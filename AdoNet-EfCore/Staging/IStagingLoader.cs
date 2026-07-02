using System.Collections.Generic;
using System.Threading.Tasks;
using AdoNet_EfCore.Models.Csv;

namespace AdoNet_EfCore.Staging;

/// <summary>
/// Contrato para la carga rápida y cruda de los datos CSV en el esquema de Staging (Stg).
/// </summary>
public interface IStagingLoader
{
    /// <summary>
    /// Limpia la tabla Stg.Customers y realiza una carga masiva de clientes.
    /// </summary>
    Task LoadCustomersAsync(IEnumerable<CustomerCsv> customers, int batchId);

    /// <summary>
    /// Limpia la tabla Stg.Products y realiza una carga masiva de productos.
    /// </summary>
    Task LoadProductsAsync(IEnumerable<ProductCsv> products, int batchId);

    /// <summary>
    /// Limpia la tabla Stg.Orders y realiza una carga masiva de órdenes.
    /// </summary>
    Task LoadOrdersAsync(IEnumerable<OrderCsv> orders, int batchId);

    /// <summary>
    /// Limpia la tabla Stg.OrderDetails y realiza una carga masiva de detalles de órdenes.
    /// </summary>
    Task LoadOrderDetailsAsync(IEnumerable<OrderDetailCsv> orderDetails, int batchId);
}
