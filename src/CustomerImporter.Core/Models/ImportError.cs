namespace CustomerImporter.Core.Models;

public class ImportError
{
    public int Row { get; init; }
    public string RawData { get; init; } = "";
    public List<string> Messages { get; init; } = [];
}
