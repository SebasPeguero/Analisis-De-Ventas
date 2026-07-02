using CsvHelper.Configuration.Attributes;

namespace AdoNet_EfCore.Models.Csv;

public class OrderDetailCsv
{
    [Name("OrderID")]
    public int OrderId { get; set; }

    [Name("ProductID")]
    public int ProductId { get; set; }

    [Name("Quantity")]
    public int Quantity { get; set; }

    [Name("TotalPrice")]
    public decimal TotalPrice { get; set; }

    public int BatchID { get; set; }
}