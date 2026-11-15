using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using B2B.Core.Data;
using B2B.Core.DTOs;
using B2B.Core.Entities;
using Microsoft.EntityFrameworkCore;

namespace B2B.Core.Services
{
    public class OrderService : IOrderService
    {
        private readonly B2BDbContext _context;

        public OrderService(B2BDbContext context)
        {
            _context = context;
        }

        public decimal CalculateOrderTotal(IEnumerable<(int quantity, decimal unitPrice)> lines)
        {
            if (lines == null) return 0m;
            decimal total = 0m;
            foreach (var line in lines)
            {
                if (line.quantity < 0 || line.unitPrice < 0)
                    throw new ArgumentException("Quantity and price must be non-negative.");
                total += line.quantity * line.unitPrice;
            }
            return total;
        }

        public async Task<OrderResult> PlaceOrderAsync(int agentId, IEnumerable<OrderItemDto> items)
        {
            if (items == null || !items.Any())
            {
                return new OrderResult { Success = false, Message = "Order must contain at least one item." };
            }

            // Consolidate duplicate items in the order
            var consolidatedItems = items
                .GroupBy(i => i.ItemID)
                .Select(g => new OrderItemDto
                {
                    ItemID = g.Key,
                    Quantity = g.Sum(x => x.Quantity)
                })
                .ToList();

            // Validate quantities
            foreach (var item in consolidatedItems)
            {
                if (item.Quantity <= 0)
                {
                    return new OrderResult
                    {
                        Success = false,
                        Message = $"Invalid quantity for ItemID {item.ItemID}. Quantity must be greater than zero."
                    };
                }
            }

            // Check agent exists
            var agent = await _context.Agents.FindAsync(agentId);
            if (agent == null)
            {
                return new OrderResult { Success = false, Message = $"Agent with ID {agentId} not found." };
            }

            // Pre-validate stock availability for all line items before creating the order
            var validatedItems = new List<(Item dbItem, int quantity)>();
            foreach (var itemDto in consolidatedItems)
            {
                var dbItem = await _context.Items.FindAsync(itemDto.ItemID);
                if (dbItem == null)
                {
                    return new OrderResult
                    {
                        Success = false,
                        Message = $"Item with ID {itemDto.ItemID} not found."
                    };
                }

                if (dbItem.StockQuantity < itemDto.Quantity)
                {
                    return new OrderResult
                    {
                        Success = false,
                        Message = $"Insufficient stock for item '{dbItem.ItemName}'. Available: {dbItem.StockQuantity}, Requested: {itemDto.Quantity}."
                    };
                }

                validatedItems.Add((dbItem, itemDto.Quantity));
            }

            // Execute order creation and stock deduction within a database transaction
            var strategy = _context.Database.CreateExecutionStrategy();

            return await strategy.ExecuteAsync(async () =>
            {
                var useTransaction = _context.Database.IsRelational();
                var transaction = useTransaction ? await _context.Database.BeginTransactionAsync() : null;

                try
                {
                    var order = new Order
                    {
                        AgentID = agentId,
                        OrderDate = DateTime.UtcNow,
                        OrderStatus = "Pending"
                    };

                    _context.Orders.Add(order);
                    await _context.SaveChangesAsync();

                    decimal totalAmount = 0m;

                    foreach (var (dbItem, quantity) in validatedItems)
                    {
                        // Update stock quantity
                        dbItem.StockQuantity -= quantity;

                        var detail = new OrderDetail
                        {
                            OrderID = order.OrderID,
                            ItemID = dbItem.ItemID,
                            Quantity = quantity,
                            UnitAmount = dbItem.Price
                        };

                        _context.OrderDetails.Add(detail);
                        totalAmount += detail.Quantity * detail.UnitAmount;
                    }

                    await _context.SaveChangesAsync();

                    if (transaction != null)
                    {
                        await transaction.CommitAsync();
                    }

                    return new OrderResult
                    {
                        Success = true,
                        OrderID = order.OrderID,
                        TotalAmount = totalAmount,
                        Message = "Order placed successfully and stock deducted."
                    };
                }
                catch (Exception ex)
                {
                    if (transaction != null)
                    {
                        await transaction.RollbackAsync();
                    }
                    return new OrderResult
                    {
                        Success = false,
                        Message = $"Transaction rolled back due to error: {ex.Message}"
                    };
                }
            });
        }
    }
}
