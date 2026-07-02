using System.Collections.Generic;
using System.Threading.Tasks;
using AnalisisDeVentas.Models;
using AnalisisDeVentas.Models.Csv;

namespace AnalisisDeVentas.Loading.Master;

public interface IProductMasterLoader
{
    Task<TableSummary> LoadCategoriesAsync(IEnumerable<ProductCsv> products);
    Task<TableSummary> LoadProductsAsync(IEnumerable<ProductCsv> products);
}
