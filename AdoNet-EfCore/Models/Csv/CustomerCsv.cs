using CsvHelper.Configuration.Attributes;

namespace AdoNet_EfCore.Models.Csv;

public class CustomerCsv
{
    [Name("CustomerID")]
    public int CustomerId { get; set; }

    [Name("FirstName")]
    public string FirstName { get; set; } = null!;

    [Name("LastName")]
    public string LastName { get; set; } = null!;

    [Name("Email")]
    public string Email { get; set; } = null!;

    [Name("Phone")]
    public string? Phone { get; set; }

    [Name("City")]
    public string City { get; set; } = null!;

    [Name("Country")]
    public string Country { get; set; } = null!;

    public int BatchID { get; set; }
}