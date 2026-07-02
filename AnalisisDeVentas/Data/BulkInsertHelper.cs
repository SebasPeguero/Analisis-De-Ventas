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

        var dbColumns = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var parts = destinationTableName.Split('.');
        var schema = parts.Length > 1 ? parts[0] : "dbo";
        var table = parts.Length > 1 ? parts[1] : parts[0];

        using (var cmd = connection.CreateCommand())
        {
            cmd.CommandText = "SELECT COLUMN_NAME FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_SCHEMA = @schema AND TABLE_NAME = @table;";
            cmd.Parameters.AddWithValue("@schema", schema);
            cmd.Parameters.AddWithValue("@table", table);
            using var reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                dbColumns.Add(reader.GetString(0));
            }
        }

        using var bulkCopy = new SqlBulkCopy(connection)
        {
            DestinationTableName = destinationTableName,
            BulkCopyTimeout = 300,
            BatchSize = 5000
        };

        foreach (DataColumn column in dataTable.Columns)
        {
            foreach (var dbCol in dbColumns)
            {
                if (string.Equals(column.ColumnName, dbCol, StringComparison.OrdinalIgnoreCase))
                {
                    bulkCopy.ColumnMappings.Add(column.ColumnName, dbCol);
                    break;
                }
            }
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