using System.Collections.Generic;
using System.Threading.Tasks;
using AdoNet_EfCore.Data;
using AdoNet_EfCore.Models.Csv;
using Microsoft.Data.SqlClient;

namespace AdoNet_EfCore.Staging;




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
}