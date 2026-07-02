using System;
using CsvHelper.Configuration.Attributes;

namespace AdoNet_EfCore.Models.Csv;

public class OrderCsv
{
    [Name("OrderID")]
    public int OrderId { get; set; }

    [Name("CustomerID")]
    public int CustomerId { get; set; }

    [Name("OrderDate")]
    public DateTime OrderDate { get; set; }

    [Name("Status")]
    public string Status { get; set; } = null!;

    public int BatchID { get; set; }
}
