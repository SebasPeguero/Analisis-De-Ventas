using System.Collections.Generic;
using System.Threading.Tasks;
using AnalisisDeVentas.Models;
using AnalisisDeVentas.Models.Csv;

namespace AnalisisDeVentas.Loading.Master;

public interface IOrderMasterLoader
{
    Task<TableSummary> LoadOrderStatusAsync(IEnumerable<OrderCsv> orders);
    Task<TableSummary> LoadOrdersAsync(IEnumerable<OrderCsv> orders);
    Task<TableSummary> LoadOrderDetailsAsync(IEnumerable<OrderDetailCsv> orderDetails);
}
