using System.Text;
using Microsoft.AspNetCore.Mvc.Testing;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

// Base class for all API integration tests.
// Each test class that inherits from this gets its own isolated store file,
// so tests never interfere with each other.
public class ApiTestBase : IDisposable
{
    protected readonly HttpClient Client;
    protected readonly string StorePath;

    private readonly WebApplicationFactory<Program> _factory;

    public ApiTestBase()
    {
        // Each test instance gets its own unique temp file
        StorePath = Path.Combine(Path.GetTempPath(), $"clientes_test_{Guid.NewGuid()}.db");

        _factory = new WebApplicationFactory<Program>()
            .WithWebHostBuilder(builder =>
            {
                // Tell the API to use our isolated temp file instead of the real one
                builder.UseSetting("StorePath", StorePath);
            });

        Client = _factory.CreateClient();
    }

    // Writes customers directly to the store file, bypassing the API.
    // Use this to set up preconditions for GET and DELETE tests.
    protected void SeedCliente(string dni, string nombre, string apellidos,
        string fechaNacimiento, string telefono, string email)
    {
        var existing = LoadRawStore();
        existing[dni] = new
        {
            Dni = dni,
            Nombre = nombre,
            Apellidos = apellidos,
            FechaNacimiento = fechaNacimiento,
            Telefono = telefono,
            Email = email
        };
        File.WriteAllText(StorePath, JsonConvert.SerializeObject(existing, Formatting.Indented));
    }

    private Dictionary<string, object> LoadRawStore()
    {
        if (!File.Exists(StorePath))
            return new Dictionary<string, object>();

        var json = File.ReadAllText(StorePath);
        return JsonConvert.DeserializeObject<Dictionary<string, object>>(json)
               ?? new Dictionary<string, object>();
    }

    // Builds a camelCase JSON body for HTTP POST requests
    protected static StringContent JsonBody(object obj)
    {
        var settings = new JsonSerializerSettings
        {
            ContractResolver = new CamelCasePropertyNamesContractResolver()
        };
        var json = JsonConvert.SerializeObject(obj, settings);
        return new StringContent(json, Encoding.UTF8, "application/json");
    }

    public void Dispose()
    {
        Client.Dispose();
        _factory.Dispose();
        if (File.Exists(StorePath))
            File.Delete(StorePath);
    }
}
