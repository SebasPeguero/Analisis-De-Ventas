using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using AnalisisDeVentas.Data;
using AnalisisDeVentas.Models.Csv;
using Microsoft.Data.SqlClient;

namespace AnalisisDeVentas.Staging;

public class StagingLoader : IStagingLoader
{
    private readonly AdoConnectionFactory _connectionFactory;
    private readonly BulkInsertHelper _bulkInsertHelper;

    public StagingLoader(AdoConnectionFactory connectionFactory, BulkInsertHelper bulkInsertHelper)
    {
        _connectionFactory = connectionFactory;
        _bulkInsertHelper = bulkInsertHelper;
    }

    private async Task TruncateTableAsync(string tableName)
    {
        using var conn = _connectionFactory.CreateConnection();
        await conn.OpenAsync();
        using var cmd = conn.CreateCommand();
        cmd.CommandText = $"TRUNCATE TABLE {tableName};";
        await cmd.ExecuteNonQueryAsync();
    }

    public async Task LoadCustomersAsync(IEnumerable<CustomerCsv> customers, int batchId)
    {
        await TruncateTableAsync("Stg.Customers");
        foreach (var item in customers)
        {
            item.BatchID = batchId;
        }
        await _bulkInsertHelper.BulkInsertAsync(customers, "Stg.Customers");
    }

    public async Task LoadProductsAsync(IEnumerable<ProductCsv> products, int batchId)
    {
        await TruncateTableAsync("Stg.Products");
        foreach (var item in products)
        {
            item.BatchID = batchId;
        }
        await _bulkInsertHelper.BulkInsertAsync(products, "Stg.Products");
    }

    public async Task LoadOrdersAsync(IEnumerable<OrderCsv> orders, int batchId)
    {
        await TruncateTableAsync("Stg.Orders");
        foreach (var item in orders)
        {
            item.BatchID = batchId;
        }
        await _bulkInsertHelper.BulkInsertAsync(orders, "Stg.Orders");
    }

    public async Task LoadOrderDetailsAsync(IEnumerable<OrderDetailCsv> orderDetails, int batchId)
    {
        await TruncateTableAsync("Stg.OrderDetails");
        foreach (var item in orderDetails)
        {
            item.BatchID = batchId;
        }
        await _bulkInsertHelper.BulkInsertAsync(orderDetails, "Stg.OrderDetails");
    }

    public async Task<IEnumerable<CustomerCsv>> GetCustomersAsync()
    {
        var list = new List<CustomerCsv>();
        using var conn = _connectionFactory.CreateConnection();
        await conn.OpenAsync();
        using var cmd = conn.CreateCommand();
        cmd.CommandText = "SELECT CustomerID, FirstName, LastName, Email, Phone, City, Country, BatchID FROM Stg.Customers;";
        using var reader = await cmd.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            list.Add(new CustomerCsv
            {
                CustomerId = reader.IsDBNull(0) ? 0 : reader.GetInt32(0),
                FirstName = reader.IsDBNull(1) ? string.Empty : reader.GetString(1),
                LastName = reader.IsDBNull(2) ? string.Empty : reader.GetString(2),
                Email = reader.IsDBNull(3) ? string.Empty : reader.GetString(3),
                Phone = reader.IsDBNull(4) ? null : reader.GetString(4),
                City = reader.IsDBNull(5) ? string.Empty : reader.GetString(5),
                Country = reader.IsDBNull(6) ? string.Empty : reader.GetString(6),
                BatchID = reader.IsDBNull(7) ? 0 : reader.GetInt32(7)
            });
        }
        return list;
    }

    public async Task<IEnumerable<ProductCsv>> GetProductsAsync()
    {
        var list = new List<ProductCsv>();
        using var conn = _connectionFactory.CreateConnection();
        await conn.OpenAsync();
        using var cmd = conn.CreateCommand();
        cmd.CommandText = "SELECT ProductID, ProductName, Category, Price, Stock, BatchID FROM Stg.Products;";
        using var reader = await cmd.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            list.Add(new ProductCsv
            {
                ProductId = reader.IsDBNull(0) ? 0 : reader.GetInt32(0),
                ProductName = reader.IsDBNull(1) ? string.Empty : reader.GetString(1),
                Category = reader.IsDBNull(2) ? string.Empty : reader.GetString(2),
                Price = reader.IsDBNull(3) ? 0m : reader.GetDecimal(3),
                Stock = reader.IsDBNull(4) ? 0 : reader.GetInt32(4),
                BatchID = reader.IsDBNull(5) ? 0 : reader.GetInt32(5)
            });
        }
        return list;
    }

    public async Task<IEnumerable<OrderCsv>> GetOrdersAsync()
    {
        var list = new List<OrderCsv>();
        using var conn = _connectionFactory.CreateConnection();
        await conn.OpenAsync();
        using var cmd = conn.CreateCommand();
        cmd.CommandText = "SELECT OrderID, CustomerID, OrderDate, Status, BatchID FROM Stg.Orders;";
        using var reader = await cmd.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            list.Add(new OrderCsv
            {
                OrderId = reader.IsDBNull(0) ? 0 : reader.GetInt32(0),
                CustomerId = reader.IsDBNull(1) ? 0 : reader.GetInt32(1),
                OrderDate = reader.IsDBNull(2) ? DateTime.MinValue : reader.GetDateTime(2),
                Status = reader.IsDBNull(3) ? string.Empty : reader.GetString(3),
                BatchID = reader.IsDBNull(4) ? 0 : reader.GetInt32(4)
            });
        }
        return list;
    }

    public async Task<IEnumerable<OrderDetailCsv>> GetOrderDetailsAsync()
    {
        var list = new List<OrderDetailCsv>();
        using var conn = _connectionFactory.CreateConnection();
        await conn.OpenAsync();
        using var cmd = conn.CreateCommand();
        cmd.CommandText = "SELECT OrderID, ProductID, Quantity, TotalPrice, BatchID FROM Stg.OrderDetails;";
        using var reader = await cmd.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            list.Add(new OrderDetailCsv
            {
                OrderId = reader.IsDBNull(0) ? 0 : reader.GetInt32(0),
                ProductId = reader.IsDBNull(1) ? 0 : reader.GetInt32(1),
                Quantity = reader.IsDBNull(2) ? 0 : reader.GetInt32(2),
                TotalPrice = reader.IsDBNull(3) ? 0m : reader.GetDecimal(3),
                BatchID = reader.IsDBNull(4) ? 0 : reader.GetInt32(4)
            });
        }
        return list;
    }
}