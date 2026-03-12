using CustomerImporter.Core.Models;
using CustomerImporter.Core.Services;

namespace CustomerImporter.Core.Tests;

public class JsonCustomerExporterTests
{
    private static Customer MakeCustomer(string dni = "12345678A") => new()
    {
        Dni = dni,
        Nombre = "Joan",
        Apellidos = "Garcia Puig",
        FechaNacimiento = "15/05/1990",
        Telefono = "612345678",
        Email = "joan@example.com"
    };

    [Fact]
    public void Export_EmptyList_ReturnsEmptyArray()
    {
        var json = JsonCustomerExporter.Export([]);

        Assert.Equal("[]", json.Trim());
    }

    [Fact]
    public void Export_SingleCustomer_ContainsAllFields()
    {
        var json = JsonCustomerExporter.Export([MakeCustomer()]);

        Assert.Contains("\"dni\": \"12345678A\"", json);
        Assert.Contains("\"nombre\": \"Joan\"", json);
        Assert.Contains("\"apellidos\": \"Garcia Puig\"", json);
        Assert.Contains("\"fechaNacimiento\": \"15/05/1990\"", json);
        Assert.Contains("\"telefono\": \"612345678\"", json);
        Assert.Contains("\"email\": \"joan@example.com\"", json);
    }

    [Fact]
    public void Export_MultipleCustomers_ReturnsJsonArray()
    {
        var customers = new List<Customer>
        {
            MakeCustomer("12345678A"),
            MakeCustomer("87654321B")
        };

        var json = JsonCustomerExporter.Export(customers);

        Assert.Contains("12345678A", json);
        Assert.Contains("87654321B", json);
    }

    [Fact]
    public void Export_Roundtrip_JsonImportRecoversData()
    {
        var original = new List<Customer> { MakeCustomer() };

        var json = JsonCustomerExporter.Export(original);
        var result = JsonCustomerImporter.Import(json);

        Assert.Single(result.Customers);
        Assert.Equal("12345678A", result.Customers[0].Dni);
        Assert.Equal("Joan", result.Customers[0].Nombre);
    }
}
