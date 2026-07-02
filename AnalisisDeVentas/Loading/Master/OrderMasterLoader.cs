using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AnalisisDeVentas.Data;
using AnalisisDeVentas.Models;
using AnalisisDeVentas.Models.Csv;
using AnalisisDeVentas.Models.Entities.Master;
using AnalisisDeVentas.Models.Entities.Sales;
using Microsoft.EntityFrameworkCore;

namespace AnalisisDeVentas.Loading.Master;

public class OrderMasterLoader : IOrderMasterLoader
{
    private readonly SistemaAnalisisVentasContext _context;

    public OrderMasterLoader(SistemaAnalisisVentasContext context)
    {
        _context = context;
    }

    public async Task<TableSummary> LoadOrderStatusAsync(IEnumerable<OrderCsv> orders)
    {
        var summary = new TableSummary { TableName = "Master.OrderStatus" };
        var rawStatuses = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        foreach (var o in orders)
        {
            var status = o.Status?.Trim();
            if (!string.IsNullOrEmpty(status))
            {
                rawStatuses.Add(status);
            }
        }

        summary.Processed = rawStatuses.Count;

        var existingStatuses = await _context.OrderStatuses
            .ToDictionaryAsync(s => s.StatusName, s => s.StatusId, StringComparer.OrdinalIgnoreCase);

        var toInsert = new List<OrderStatus>();
        foreach (var statusName in rawStatuses)
        {
            if (!existingStatuses.ContainsKey(statusName))
            {
                var status = new OrderStatus { StatusName = statusName };
                toInsert.Add(status);
                summary.Inserted++;
            }
        }

        if (toInsert.Count > 0)
        {
            await _context.OrderStatuses.AddRangeAsync(toInsert);
            await _context.SaveChangesAsync();
        }

        return summary;
    }

    public async Task<TableSummary> LoadOrdersAsync(IEnumerable<OrderCsv> orders)
    {
        var summary = new TableSummary { TableName = "Sales.Orders" };
        
        var uniqueOrders = new List<OrderCsv>();
        var seenIds = new HashSet<int>();
        foreach (var o in orders)
        {
            if (seenIds.Add(o.OrderId))
            {
                uniqueOrders.Add(o);
            }
        }

        summary.Processed = uniqueOrders.Count;

        var statuses = await _context.OrderStatuses
            .ToDictionaryAsync(s => s.StatusName, s => s.StatusId, StringComparer.OrdinalIgnoreCase);

        var customers = await _context.Customers
            .ToDictionaryAsync(c => c.CustomerCode, c => c.CustomerId, StringComparer.OrdinalIgnoreCase);

        var existingOrderCodes = new HashSet<string>(await _context.Orders
            .Select(o => o.OrderCode)
            .ToListAsync());

        var toInsert = new List<Order>();
        foreach (var csv in uniqueOrders)
        {
            var code = csv.OrderId.ToString();
            if (existingOrderCodes.Contains(code))
            {
                continue;
            }

            if (csv.OrderDate == DateTime.MinValue)
            {
                summary.Rejected++;
                continue;
            }

            var customerCode = csv.CustomerId.ToString();
            if (!customers.TryGetValue(customerCode, out int customerId))
            {
                summary.Rejected++;
                continue;
            }

            var statusName = csv.Status?.Trim();
            if (string.IsNullOrEmpty(statusName) || !statuses.TryGetValue(statusName, out int statusId))
            {
                summary.Rejected++;
                continue;
            }

            var order = new Order
            {
                OrderCode = code,
                CustomerId = customerId,
                OrderDate = csv.OrderDate,
                StatusId = statusId,
                CreatedAt = DateTime.Now
            };

            toInsert.Add(order);
            summary.Inserted++;
        }

        if (toInsert.Count > 0)
        {
            await _context.Orders.AddRangeAsync(toInsert);
            await _context.SaveChangesAsync();
        }

        return summary;
    }

    public async Task<TableSummary> LoadOrderDetailsAsync(IEnumerable<OrderDetailCsv> orderDetails)
    {
        var summary = new TableSummary { TableName = "Sales.OrderDetails" };
        
        var listDetails = orderDetails.ToList();
        summary.Processed = listDetails.Count;

        var orders = await _context.Orders
            .ToDictionaryAsync(o => o.OrderCode, o => o.OrderId, StringComparer.OrdinalIgnoreCase);

        var products = await _context.Products
            .ToDictionaryAsync(p => p.ProductCode, p => p.ProductId, StringComparer.OrdinalIgnoreCase);

        var existingKeys = new HashSet<string>(await _context.OrderDetails
            .Select(d => $"{d.OrderId}|{d.ProductId}|{d.Quantity}|{d.Total}")
            .ToListAsync());

        var toInsert = new List<OrderDetail>();
        foreach (var csv in listDetails)
        {
            if (csv.Quantity <= 0 || csv.TotalPrice < 0)
            {
                summary.Rejected++;
                continue;
            }

            var orderCode = csv.OrderId.ToString();
            if (!orders.TryGetValue(orderCode, out int orderId))
            {
                summary.Rejected++;
                continue;
            }

            var productCode = csv.ProductId.ToString();
            if (!products.TryGetValue(productCode, out int productId))
            {
                summary.Rejected++;
                continue;
            }

            var key = $"{orderId}|{productId}|{csv.Quantity}|{csv.TotalPrice}";
            if (existingKeys.Contains(key))
            {
                continue;
            }

            decimal unitPrice = csv.TotalPrice / csv.Quantity;

            var detail = new OrderDetail
            {
                OrderId = orderId,
                ProductId = productId,
                Quantity = csv.Quantity,
                UnitPrice = unitPrice,
                Total = csv.TotalPrice
            };

            toInsert.Add(detail);
            summary.Inserted++;
        }

        if (toInsert.Count > 0)
        {
            await _context.OrderDetails.AddRangeAsync(toInsert);
            await _context.SaveChangesAsync();
        }

        return summary;
    }
}
