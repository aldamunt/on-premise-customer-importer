using System.Net;
using Newtonsoft.Json.Linq;

public class PostClienteTests : ApiTestBase
{
    // Reusable valid customer data for POST tests
    private static readonly object ClienteValido = new
    {
        Dni = "12345678A",
        Nombre = "Juan",
        Apellidos = "García",
        FechaNacimiento = "15/01/1990",
        Telefono = "612345678",
        Email = "juan@example.com"
    };

    [Fact]
    public async Task PostCliente_ValidData_Returns201()
    {
        var response = await Client.PostAsync("/clientes", JsonBody(ClienteValido));

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
    }

    [Fact]
    public async Task PostCliente_ValidData_LocationHeaderPointsToNewClient()
    {
        var response = await Client.PostAsync("/clientes", JsonBody(ClienteValido));

        Assert.Equal("/clientes/12345678A", response.Headers.Location?.ToString());
    }

    [Fact]
    public async Task PostCliente_ValidData_BodyContainsCreatedClient()
    {
        var response = await Client.PostAsync("/clientes", JsonBody(ClienteValido));

        var body = await response.Content.ReadAsStringAsync();
        var cliente = JObject.Parse(body);
        Assert.Equal("12345678A", cliente["dni"]?.ToString());
        Assert.Equal("Juan", cliente["nombre"]?.ToString());
    }

    [Fact]
    public async Task PostCliente_DuplicateDni_Returns400()
    {
        SeedCliente("12345678A", "Juan", "García", "15/01/1990", "612345678", "juan@example.com");

        var response = await Client.PostAsync("/clientes", JsonBody(ClienteValido));

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task PostCliente_DuplicateDni_ErrorBodyMentionsDniField()
    {
        SeedCliente("12345678A", "Juan", "García", "15/01/1990", "612345678", "juan@example.com");

        var response = await Client.PostAsync("/clientes", JsonBody(ClienteValido));

        var body = await response.Content.ReadAsStringAsync();
        var errors = JObject.Parse(body)["errors"] as JArray;
        Assert.NotNull(errors);
        Assert.Contains(errors, e => e["field"]?.ToString() == "Dni");
    }

    [Fact]
    public async Task PostCliente_InvalidData_Returns400()
    {
        var clienteInvalido = new
        {
            Dni = "INVALIDO",
            Nombre = "Juan",
            Apellidos = "García",
            FechaNacimiento = "15/01/1990",
            Telefono = "612345678",
            Email = "no-es-un-email"
        };

        var response = await Client.PostAsync("/clientes", JsonBody(clienteInvalido));

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task PostCliente_InvalidData_ErrorBodyContainsFieldLevelErrors()
    {
        var clienteInvalido = new
        {
            Dni = "INVALIDO",
            Nombre = "Juan",
            Apellidos = "García",
            FechaNacimiento = "15/01/1990",
            Telefono = "612345678",
            Email = "no-es-un-email"
        };

        var response = await Client.PostAsync("/clientes", JsonBody(clienteInvalido));

        var body = await response.Content.ReadAsStringAsync();
        var errors = JObject.Parse(body)["errors"] as JArray;
        Assert.NotNull(errors);
        Assert.Contains(errors, e => e["field"]?.ToString() == "Dni");
        Assert.Contains(errors, e => e["field"]?.ToString() == "Email");
    }

    [Fact]
    public async Task PostCliente_ValidData_ClienteRetrievableAfterwards()
    {
        await Client.PostAsync("/clientes", JsonBody(ClienteValido));

        var response = await Client.GetAsync("/clientes/12345678A");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await response.Content.ReadAsStringAsync();
        var cliente = JObject.Parse(body);
        Assert.Equal("12345678A", cliente["dni"]?.ToString());
    }
}
