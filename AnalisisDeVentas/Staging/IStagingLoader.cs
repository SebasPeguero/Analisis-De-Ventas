using System.Collections.Generic;
using System.Threading.Tasks;
using AnalisisDeVentas.Models.Csv;

namespace AnalisisDeVentas.Staging;




public interface IStagingLoader
{
    
    
    
    Task LoadCustomersAsync(IEnumerable<CustomerCsv> customers, int batchId);

    
    
    
    Task LoadProductsAsync(IEnumerable<ProductCsv> products, int batchId);

    
    
    
    Task LoadOrdersAsync(IEnumerable<OrderCsv> orders, int batchId);

    
    
    
    Task LoadOrderDetailsAsync(IEnumerable<OrderDetailCsv> orderDetails, int batchId);
}