using CustomerImporter.Core.Models;
using Newtonsoft.Json;

namespace CustomerImporter.Core.Services;

public class CustomerStore
{
    private readonly string _filePath;

    public CustomerStore(string filePath) => _filePath = filePath;

    public Dictionary<string, Customer> Load()
    {
        if (!File.Exists(_filePath))
            return new Dictionary<string, Customer>();

        var json = File.ReadAllText(_filePath);
        return JsonConvert.DeserializeObject<Dictionary<string, Customer>>(json)
               ?? new Dictionary<string, Customer>();
    }

    public void Save(Dictionary<string, Customer> customers)
    {
        var dir = Path.GetDirectoryName(_filePath);
        if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
            Directory.CreateDirectory(dir);

        var json = JsonConvert.SerializeObject(customers, Formatting.Indented);
        File.WriteAllText(_filePath, json);
    }

    public Dictionary<string, Customer> Merge(
        Dictionary<string, Customer> existing, List<Customer> incoming)
    {
        foreach (var customer in incoming)
        {
            if (customer.Dni is null) continue;
            existing[customer.Dni] = customer;
        }
        return existing;
    }
}
