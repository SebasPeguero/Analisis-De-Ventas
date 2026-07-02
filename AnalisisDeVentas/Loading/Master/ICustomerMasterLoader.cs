using System.Collections.Generic;
using System.Threading.Tasks;
using AnalisisDeVentas.Models;
using AnalisisDeVentas.Models.Csv;

namespace AnalisisDeVentas.Loading.Master;

public interface ICustomerMasterLoader
{
    Task<TableSummary> LoadCountriesAsync(IEnumerable<CustomerCsv> customers);
    Task<TableSummary> LoadCitiesAsync(IEnumerable<CustomerCsv> customers);
    Task<TableSummary> LoadCustomersAsync(IEnumerable<CustomerCsv> customers);
}
