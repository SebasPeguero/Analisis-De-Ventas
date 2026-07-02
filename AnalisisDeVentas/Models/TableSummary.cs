namespace AnalisisDeVentas.Models;

public class TableSummary
{
    public string TableName { get; set; } = null!;
    public int Processed { get; set; }
    public int Inserted { get; set; }
    public int Rejected { get; set; }
}
