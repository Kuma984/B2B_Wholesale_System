using System.Collections.Generic;
using System.Threading.Tasks;
using B2B.Core.DTOs;
using B2B.Core.Entities;

namespace B2B.Core.Services
{
    public interface IOrderService
    {
        Task<OrderResult> PlaceOrderAsync(int agentId, IEnumerable<OrderItemDto> items);
        decimal CalculateOrderTotal(IEnumerable<(int quantity, decimal unitPrice)> lines);
    }
}
