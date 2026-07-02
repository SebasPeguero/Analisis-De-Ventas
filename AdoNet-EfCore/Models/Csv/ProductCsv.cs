using CsvHelper.Configuration.Attributes;

namespace AdoNet_EfCore.Models.Csv;

public class ProductCsv
{
    [Name("ProductID")]
    public int ProductId { get; set; }

    [Name("ProductName")]
    public string ProductName { get; set; } = null!;

    [Name("Category")]
    public string Category { get; set; } = null!;

    [Name("Price")]
    public decimal Price { get; set; }

    [Name("Stock")]
    public int Stock { get; set; }

    public int BatchID { get; set; }
}