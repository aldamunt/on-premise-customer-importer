namespace CustomerImporter.Core.Models;

public class ImportResult
{
    public List<Customer> Customers { get; set; } = [];
    public List<string> Errors { get; set; } = [];
}
