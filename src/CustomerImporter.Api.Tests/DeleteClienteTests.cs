using System.Net;

public class DeleteClienteTests : ApiTestBase
{
    [Fact]
    public async Task DeleteCliente_Exists_Returns204()
    {
        SeedCliente("12345678A", "Juan", "García", "15/01/1990", "612345678", "juan@example.com");

        var response = await Client.DeleteAsync("/clientes/12345678A");

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
    }

    [Fact]
    public async Task DeleteCliente_NotExists_Returns404()
    {
        var response = await Client.DeleteAsync("/clientes/99999999Z");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task DeleteCliente_Exists_ClientNoLongerRetrievable()
    {
        SeedCliente("12345678A", "Juan", "García", "15/01/1990", "612345678", "juan@example.com");

        await Client.DeleteAsync("/clientes/12345678A");

        var response = await Client.GetAsync("/clientes/12345678A");
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task DeleteCliente_WhenMultipleClients_OnlyDeletesRequested()
    {
        SeedCliente("12345678A", "Juan", "García", "15/01/1990", "612345678", "juan@example.com");
        SeedCliente("87654321B", "Ana", "López", "20/03/1985", "612345679", "ana@example.com");

        await Client.DeleteAsync("/clientes/12345678A");

        var remaining = await Client.GetAsync("/clientes/87654321B");
        Assert.Equal(HttpStatusCode.OK, remaining.StatusCode);
    }
}
