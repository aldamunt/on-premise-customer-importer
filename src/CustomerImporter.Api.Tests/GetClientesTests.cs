using System.Net;
using Newtonsoft.Json.Linq;

public class GetClientesTests : ApiTestBase
{
    [Fact]
    public async Task GetClientes_EmptyStore_Returns200WithEmptyArray()
    {
        var response = await Client.GetAsync("/clientes");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await response.Content.ReadAsStringAsync();
        var array = JArray.Parse(body);
        Assert.Empty(array);
    }

    [Fact]
    public async Task GetClientes_WithTwoClients_Returns200WithBothClients()
    {
        SeedCliente("12345678A", "Juan", "García", "15/01/1990", "612345678", "juan@example.com");
        SeedCliente("87654321B", "Ana", "López", "20/03/1985", "612345679", "ana@example.com");

        var response = await Client.GetAsync("/clientes");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await response.Content.ReadAsStringAsync();
        var array = JArray.Parse(body);
        Assert.Equal(2, array.Count);
    }

    [Fact]
    public async Task GetClientes_ResponseUsesCamelCaseProperties()
    {
        SeedCliente("12345678A", "Juan", "García", "15/01/1990", "612345678", "juan@example.com");

        var response = await Client.GetAsync("/clientes");

        var body = await response.Content.ReadAsStringAsync();
        var array = JArray.Parse(body);
        var cliente = array[0] as JObject;
        Assert.NotNull(cliente!["dni"]);
        Assert.NotNull(cliente["nombre"]);
        Assert.NotNull(cliente["apellidos"]);
        Assert.NotNull(cliente["fechaNacimiento"]);
        Assert.NotNull(cliente["telefono"]);
        Assert.NotNull(cliente["email"]);
    }
}
