using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using AnalisisDeVentas.Extraction;
using AnalisisDeVentas.Loading.Dimensional;
using AnalisisDeVentas.Loading.Master;
using AnalisisDeVentas.Models;
using AnalisisDeVentas.Models.Csv;
using AnalisisDeVentas.Staging;
using Microsoft.Extensions.Configuration;

namespace AnalisisDeVentas.Pipeline;

public class EtlOrchestrator
{
    private readonly IConfiguration _configuration;
    private readonly ICsvExtractor<CustomerCsv> _customerExtractor;
    private readonly ICsvExtractor<ProductCsv> _productExtractor;
    private readonly ICsvExtractor<OrderCsv> _orderExtractor;
    private readonly ICsvExtractor<OrderDetailCsv> _orderDetailExtractor;
    private readonly IStagingLoader _stagingLoader;
    private readonly ICustomerMasterLoader _customerMasterLoader;
    private readonly IProductMasterLoader _productMasterLoader;
    private readonly IOrderMasterLoader _orderMasterLoader;
    private readonly IDimensionBuilder _dimensionBuilder;

    public EtlOrchestrator(
        IConfiguration configuration,
        ICsvExtractor<CustomerCsv> customerExtractor,
        ICsvExtractor<ProductCsv> productExtractor,
        ICsvExtractor<OrderCsv> orderExtractor,
        ICsvExtractor<OrderDetailCsv> orderDetailExtractor,
        IStagingLoader stagingLoader,
        ICustomerMasterLoader customerMasterLoader,
        IProductMasterLoader productMasterLoader,
        IOrderMasterLoader orderMasterLoader,
        IDimensionBuilder dimensionBuilder)
    {
        _configuration = configuration;
        _customerExtractor = customerExtractor;
        _productExtractor = productExtractor;
        _orderExtractor = orderExtractor;
        _orderDetailExtractor = orderDetailExtractor;
        _stagingLoader = stagingLoader;
        _customerMasterLoader = customerMasterLoader;
        _productMasterLoader = productMasterLoader;
        _orderMasterLoader = orderMasterLoader;
        _dimensionBuilder = dimensionBuilder;
    }

    public async Task RunAsync()
    {
        var stopwatch = Stopwatch.StartNew();
        Console.WriteLine("Iniciando proceso ETL...");

        int batchId = (int)DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1)).TotalSeconds;

        var custPath = _configuration["CsvPaths:Customers"]!;
        var prodPath = _configuration["CsvPaths:Products"]!;
        var ordPath = _configuration["CsvPaths:Orders"]!;
        var detPath = _configuration["CsvPaths:OrderDetails"]!;

        Console.WriteLine("1. Extrayendo datos de archivos CSV...");
        var csvCustomers = await _customerExtractor.ExtractAsync(custPath);
        var csvProducts = await _productExtractor.ExtractAsync(prodPath);
        var csvOrders = await _orderExtractor.ExtractAsync(ordPath);
        var csvOrderDetails = await _orderDetailExtractor.ExtractAsync(detPath);

        Console.WriteLine("2. Cargando datos crudos en Staging...");
        await _stagingLoader.LoadCustomersAsync(csvCustomers, batchId);
        await _stagingLoader.LoadProductsAsync(csvProducts, batchId);
        await _stagingLoader.LoadOrdersAsync(csvOrders, batchId);
        await _stagingLoader.LoadOrderDetailsAsync(csvOrderDetails, batchId);

        Console.WriteLine("3. Recuperando datos de Staging para procesamiento...");
        var stgCustomers = await _stagingLoader.GetCustomersAsync();
        var stgProducts = await _stagingLoader.GetProductsAsync();
        var stgOrders = await _stagingLoader.GetOrdersAsync();
        var stgOrderDetails = await _stagingLoader.GetOrderDetailsAsync();

        Console.WriteLine("4. Transformando y cargando datos en esquema transaccional (Master/Sales)...");
        var summaries = new List<TableSummary>();

        summaries.Add(await _customerMasterLoader.LoadCountriesAsync(stgCustomers));
        summaries.Add(await _customerMasterLoader.LoadCitiesAsync(stgCustomers));
        summaries.Add(await _customerMasterLoader.LoadCustomersAsync(stgCustomers));

        summaries.Add(await _productMasterLoader.LoadCategoriesAsync(stgProducts));
        summaries.Add(await _productMasterLoader.LoadProductsAsync(stgProducts));

        summaries.Add(await _orderMasterLoader.LoadOrderStatusAsync(stgOrders));
        summaries.Add(await _orderMasterLoader.LoadOrdersAsync(stgOrders));
        summaries.Add(await _orderMasterLoader.LoadOrderDetailsAsync(stgOrderDetails));

        Console.WriteLine("5. Construyendo modelo dimensional y de hechos (Dim/Fact)...");
        await _dimensionBuilder.BuildDimensionsAsync();
        await _dimensionBuilder.BuildFactsAsync();

        stopwatch.Stop();

        Console.WriteLine("\n=======================================================================");
        Console.WriteLine("                     RESUMEN DE PROCESAMIENTO ETL                      ");
        Console.WriteLine("=======================================================================");
        Console.WriteLine(string.Format("{0,-25} | {1,12} | {2,12} | {3,12}", "Tabla", "Procesados", "Insertados", "Rechazados"));
        Console.WriteLine("-----------------------------------------------------------------------");
        foreach (var s in summaries)
        {
            Console.WriteLine(string.Format("{0,-25} | {1,12} | {2,12} | {3,12}", s.TableName, s.Processed, s.Inserted, s.Rejected));
        }
        Console.WriteLine("=======================================================================");
        Console.WriteLine($"Proceso ETL completado con éxito en: {stopwatch.Elapsed.TotalSeconds:F2} segundos.\n");
    }
}
