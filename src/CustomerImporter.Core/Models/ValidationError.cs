namespace CustomerImporter.Core.Models;

public class ValidationError
{
    public required string Field { get; init; }
    public required string Message { get; init; }
}
