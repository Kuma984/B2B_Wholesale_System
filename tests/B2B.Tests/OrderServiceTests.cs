using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using B2B.Core.Data;
using B2B.Core.DTOs;
using B2B.Core.Entities;
using B2B.Core.Services;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace B2B.Tests
{
    public class OrderServiceTests
    {
        private B2BDbContext CreateInMemoryDbContext(string dbName)
        {
            var options = new DbContextOptionsBuilder<B2BDbContext>()
                .UseInMemoryDatabase(databaseName: dbName)
                .Options;

            var context = new B2BDbContext(options);
            return context;
        }

        [Fact]
        public async Task PlaceOrder_WithSufficientStock_DeductsStockAndCreatesOrder()
        {
            // Arrange
            using var context = CreateInMemoryDbContext("Order_Success_Db");
            var agent = new Agent { AgentID = 1, AgentName = "Test Agent", Address = "123 St", Phone = "0123" };
            var item = new Item { ItemID = 1, ItemName = "Test Item", Size = "Box", Price = 100m, StockQuantity = 50 };

            context.Agents.Add(agent);
            context.Items.Add(item);
            await context.SaveChangesAsync();

            var service = new OrderService(context);
            var orderItems = new List<OrderItemDto>
            {
                new OrderItemDto { ItemID = 1, Quantity = 10 }
            };

            // Act
            var result = await service.PlaceOrderAsync(1, orderItems);

            // Assert
            Assert.True(result.Success);
            Assert.NotNull(result.OrderID);
            Assert.Equal(1000m, result.TotalAmount);

            var updatedItem = await context.Items.FindAsync(1);
            Assert.NotNull(updatedItem);
            Assert.Equal(40, updatedItem.StockQuantity); // 50 - 10 = 40
        }

        [Fact]
        public async Task PlaceOrder_WithInsufficientStock_BlocksOrderAndDoesNotDeductStock()
        {
            // Arrange
            using var context = CreateInMemoryDbContext("Order_Insufficient_Db");
            var agent = new Agent { AgentID = 2, AgentName = "Test Agent 2", Address = "456 St", Phone = "0456" };
            var item = new Item { ItemID = 2, ItemName = "Low Stock Item", Size = "Roll", Price = 50m, StockQuantity = 5 };

            context.Agents.Add(agent);
            context.Items.Add(item);
            await context.SaveChangesAsync();

            var service = new OrderService(context);
            var orderItems = new List<OrderItemDto>
            {
                new OrderItemDto { ItemID = 2, Quantity = 15 } // Request 15 when only 5 available
            };

            // Act
            var result = await service.PlaceOrderAsync(2, orderItems);

            // Assert
            Assert.False(result.Success);
            Assert.Contains("Insufficient stock", result.Message);

            // Verify stock is untouched
            var updatedItem = await context.Items.FindAsync(2);
            Assert.NotNull(updatedItem);
            Assert.Equal(5, updatedItem.StockQuantity);

            // Verify no order was created
            var orders = await context.Orders.ToListAsync();
            Assert.Empty(orders);
        }

        [Fact]
        public async Task PlaceOrder_WhenQuantityEqualsStock_SucceedsAndStockBecomesZero()
        {
            // Arrange
            using var context = CreateInMemoryDbContext("Order_ExactStock_Db");
            var agent = new Agent { AgentID = 3, AgentName = "Test Agent 3", Address = "789 St", Phone = "0789" };
            var item = new Item { ItemID = 3, ItemName = "Exact Stock Item", Size = "Pack", Price = 200m, StockQuantity = 10 };

            context.Agents.Add(agent);
            context.Items.Add(item);
            await context.SaveChangesAsync();

            var service = new OrderService(context);
            var orderItems = new List<OrderItemDto>
            {
                new OrderItemDto { ItemID = 3, Quantity = 10 }
            };

            // Act
            var result = await service.PlaceOrderAsync(3, orderItems);

            // Assert
            Assert.True(result.Success);
            var updatedItem = await context.Items.FindAsync(3);
            Assert.NotNull(updatedItem);
            Assert.Equal(0, updatedItem.StockQuantity);
        }

        [Fact]
        public async Task PlaceOrder_WithNegativeOrZeroQuantity_RejectsWithValidationMessage()
        {
            // Arrange
            using var context = CreateInMemoryDbContext("Order_InvalidQty_Db");
            var agent = new Agent { AgentID = 4, AgentName = "Test Agent 4", Address = "321 St", Phone = "0321" };
            context.Agents.Add(agent);
            await context.SaveChangesAsync();

            var service = new OrderService(context);
            var orderItems = new List<OrderItemDto>
            {
                new OrderItemDto { ItemID = 1, Quantity = -5 }
            };

            // Act
            var result = await service.PlaceOrderAsync(4, orderItems);

            // Assert
            Assert.False(result.Success);
            Assert.Contains("Quantity must be greater than zero", result.Message);
        }

        [Fact]
        public async Task PlaceOrder_WithDuplicateItem_CombinesQuantitiesBeforeCheckingStock()
        {
            // Arrange
            using var context = CreateInMemoryDbContext("Order_DuplicateItem_Db");
            var agent = new Agent { AgentID = 5, AgentName = "Test Agent 5", Address = "654 St", Phone = "0654" };
            var item = new Item { ItemID = 5, ItemName = "Duplicated Item", Size = "Unit", Price = 100m, StockQuantity = 15 };

            context.Agents.Add(agent);
            context.Items.Add(item);
            await context.SaveChangesAsync();

            var service = new OrderService(context);
            // 10 + 10 = 20 requested, but stock is only 15 -> should fail!
            var orderItems = new List<OrderItemDto>
            {
                new OrderItemDto { ItemID = 5, Quantity = 10 },
                new OrderItemDto { ItemID = 5, Quantity = 10 }
            };

            // Act
            var result = await service.PlaceOrderAsync(5, orderItems);

            // Assert
            Assert.False(result.Success);
            Assert.Contains("Insufficient stock", result.Message);

            // Verify stock untouched
            var updatedItem = await context.Items.FindAsync(5);
            Assert.NotNull(updatedItem);
            Assert.Equal(15, updatedItem.StockQuantity);
        }

        [Fact]
        public void CalculateOrderTotal_MultipleLines_ReturnsAccurateSum()
        {
            // Arrange
            using var context = CreateInMemoryDbContext("Order_TotalCalc_Db");
            var service = new OrderService(context);

            var lines = new List<(int quantity, decimal unitPrice)>
            {
                (5, 100m),  // 500
                (2, 250m),  // 500
                (10, 25m)   // 250
            };

            // Act
            var total = service.CalculateOrderTotal(lines);

            // Assert
            Assert.Equal(1250m, total);
        }
    }
}
