using System.Net;
using Newtonsoft.Json.Linq;

public class GetClienteByDniTests : ApiTestBase
{
    [Fact]
    public async Task GetClienteByDni_Exists_Returns200WithCorrectData()
    {
        SeedCliente("12345678A", "Juan", "García", "15/01/1990", "612345678", "juan@example.com");

        var response = await Client.GetAsync("/clientes/12345678A");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await response.Content.ReadAsStringAsync();
        var cliente = JObject.Parse(body);
        Assert.Equal("12345678A", cliente["dni"]?.ToString());
        Assert.Equal("Juan", cliente["nombre"]?.ToString());
        Assert.Equal("García", cliente["apellidos"]?.ToString());
    }

    [Fact]
    public async Task GetClienteByDni_NotExists_Returns404()
    {
        var response = await Client.GetAsync("/clientes/99999999Z");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task GetClienteByDni_WhenMultipleClients_ReturnsOnlyRequested()
    {
        SeedCliente("12345678A", "Juan", "García", "15/01/1990", "612345678", "juan@example.com");
        SeedCliente("87654321B", "Ana", "López", "20/03/1985", "612345679", "ana@example.com");

        var response = await Client.GetAsync("/clientes/87654321B");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await response.Content.ReadAsStringAsync();
        var cliente = JObject.Parse(body);
        Assert.Equal("87654321B", cliente["dni"]?.ToString());
        Assert.Equal("Ana", cliente["nombre"]?.ToString());
    }
}
