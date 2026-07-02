using System;
using System.Collections.Generic;
using System.Data;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.Data.SqlClient;

namespace AnalisisDeVentas.Data;




public class BulkInsertHelper
{
    private readonly AdoConnectionFactory _connectionFactory;

    
    
    
    
    public BulkInsertHelper(AdoConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    
    
    
    
    
    
    
    public async Task BulkInsertAsync<T>(IEnumerable<T> items, string destinationTableName)
    {
        using var dataTable = ConvertToDataTable(items);
        using var connection = _connectionFactory.CreateConnection();
        await connection.OpenAsync();

        using var bulkCopy = new SqlBulkCopy(connection)
        {
            DestinationTableName = destinationTableName,
            BulkCopyTimeout = 300, 
            BatchSize = 5000       
        };

        
        foreach (DataColumn column in dataTable.Columns)
        {
            bulkCopy.ColumnMappings.Add(column.ColumnName, column.ColumnName);
        }

        await bulkCopy.WriteToServerAsync(dataTable);
    }

    
    
    
    private DataTable ConvertToDataTable<T>(IEnumerable<T> items)
    {
        var dataTable = new DataTable(typeof(T).Name);
        var properties = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance);

        
        foreach (var prop in properties)
        {
            var propType = prop.PropertyType;
            
            if (propType.IsGenericType && propType.GetGenericTypeDefinition() == typeof(Nullable<>))
            {
                propType = Nullable.GetUnderlyingType(propType)!;
            }
            dataTable.Columns.Add(prop.Name, propType);
        }

        
        foreach (var item in items)
        {
            var values = new object?[properties.Length];
            for (int i = 0; i < properties.Length; i++)
            {
                values[i] = properties[i].GetValue(item, null) ?? DBNull.Value;
            }
            dataTable.Rows.Add(values);
        }

        return dataTable;
    }
}