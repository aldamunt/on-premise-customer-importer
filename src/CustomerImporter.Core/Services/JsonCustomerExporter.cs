using CustomerImporter.Core.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace CustomerImporter.Core.Services;

public static class JsonCustomerExporter
{
    private static readonly JsonSerializerSettings Settings = new()
    {
        Formatting = Formatting.Indented,
        ContractResolver = new CamelCasePropertyNamesContractResolver()
    };

    public static string Export(List<Customer> customers)
    {
        return JsonConvert.SerializeObject(customers, Settings);
    }
}
