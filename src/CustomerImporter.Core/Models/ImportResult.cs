namespace CustomerImporter.Core.Models;

public class ImportResult
{
    public List<Customer> Customers { get; set; } = [];
    public List<ImportError> Errors { get; set; } = [];
    public int TotalRows { get; set; }
}
