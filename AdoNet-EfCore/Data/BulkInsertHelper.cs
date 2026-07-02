using System;
using System.Collections.Generic;
using System.Data;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.Data.SqlClient;

namespace AdoNet_EfCore.Data;

/// <summary>
/// Clase auxiliar genérica para la inserción masiva de datos a alta velocidad en SQL Server mediante SqlBulkCopy.
/// </summary>
public class BulkInsertHelper
{
    private readonly AdoConnectionFactory _connectionFactory;

    /// <summary>
    /// Inicializa una nueva instancia de <see cref="BulkInsertHelper"/>.
    /// </summary>
    /// <param name="connectionFactory">Fábrica de conexiones ADO.NET.</param>
    public BulkInsertHelper(AdoConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    /// <summary>
    /// Realiza una carga masiva de la colección especificada en la tabla destino de la base de datos de forma asíncrona.
    /// </summary>
    /// <typeparam name="T">El tipo de objeto en la colección.</typeparam>
    /// <param name="items">Colección de registros a insertar.</param>
    /// <param name="destinationTableName">Nombre completo de la tabla destino (ej. "Stg.Customers").</param>
    /// <returns>Una tarea asíncrona que representa la operación.</returns>
    public async Task BulkInsertAsync<T>(IEnumerable<T> items, string destinationTableName)
    {
        using var dataTable = ConvertToDataTable(items);
        using var connection = _connectionFactory.CreateConnection();
        await connection.OpenAsync();

        using var bulkCopy = new SqlBulkCopy(connection)
        {
            DestinationTableName = destinationTableName,
            BulkCopyTimeout = 300, // 5 minutos de tiempo de espera máximo
            BatchSize = 5000       // Procesar en lotes de 5000 registros
        };

        // Mapear explícitamente las columnas por nombre para evitar problemas de orden
        foreach (DataColumn column in dataTable.Columns)
        {
            bulkCopy.ColumnMappings.Add(column.ColumnName, column.ColumnName);
        }

        await bulkCopy.WriteToServerAsync(dataTable);
    }

    /// <summary>
    /// Convierte una colección genérica a un objeto DataTable mediante reflexión.
    /// </summary>
    private DataTable ConvertToDataTable<T>(IEnumerable<T> items)
    {
        var dataTable = new DataTable(typeof(T).Name);
        var properties = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance);

        // Crear columnas en el DataTable
        foreach (var prop in properties)
        {
            var propType = prop.PropertyType;
            // Manejar tipos Nullable
            if (propType.IsGenericType && propType.GetGenericTypeDefinition() == typeof(Nullable<>))
            {
                propType = Nullable.GetUnderlyingType(propType)!;
            }
            dataTable.Columns.Add(prop.Name, propType);
        }

        // Poblar el DataTable
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
