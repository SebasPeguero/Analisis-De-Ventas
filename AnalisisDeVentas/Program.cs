using System;
using System.IO;
using System.Threading.Tasks;
using AnalisisDeVentas.Data;
using AnalisisDeVentas.Extraction;
using AnalisisDeVentas.Loading.Dimensional;
using AnalisisDeVentas.Loading.Master;
using AnalisisDeVentas.Models.Csv;
using AnalisisDeVentas.Pipeline;
using AnalisisDeVentas.Staging;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace AnalisisDeVentas;

class Program
{
    static async Task Main(string[] args)
    {
        var configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
            .Build();

        var services = new ServiceCollection();

        services.AddSingleton<IConfiguration>(configuration);

        services.AddDbContext<SistemaAnalisisVentasContext>(options =>
            options.UseSqlServer(configuration.GetConnectionString("SistemaAnalisisVentas")));

        services.AddSingleton<AdoConnectionFactory>();
        services.AddSingleton<BulkInsertHelper>();

        services.AddTransient<ICsvExtractor<CustomerCsv>, CsvExtractor<CustomerCsv>>();
        services.AddTransient<ICsvExtractor<ProductCsv>, CsvExtractor<ProductCsv>>();
        services.AddTransient<ICsvExtractor<OrderCsv>, CsvExtractor<OrderCsv>>();
        services.AddTransient<ICsvExtractor<OrderDetailCsv>, CsvExtractor<OrderDetailCsv>>();

        services.AddTransient<IStagingLoader, StagingLoader>();
        services.AddTransient<ICustomerMasterLoader, CustomerMasterLoader>();
        services.AddTransient<IProductMasterLoader, ProductMasterLoader>();
        services.AddTransient<IOrderMasterLoader, OrderMasterLoader>();
        services.AddTransient<IDimensionBuilder, DimensionBuilder>();

        services.AddTransient<EtlOrchestrator>();

        var serviceProvider = services.BuildServiceProvider();

        try
        {
            var orchestrator = serviceProvider.GetRequiredService<EtlOrchestrator>();
            await orchestrator.RunAsync();
        }
        catch (Exception ex)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"Error critico en la ejecucion del ETL: {ex.Message}");
            Console.WriteLine(ex.StackTrace);
            Console.ResetColor();
        }
    }
}