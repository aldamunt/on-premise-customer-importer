using CustomerImporter.Core.Services;
using Newtonsoft.Json;

namespace CustomerImporter.Core.Tests;

public class JsonCustomerImporterTests
{
    private const string ValidJson = """
        [
          {
            "dni": "12345678A",
            "nombre": "Joan",
            "apellidos": "Garcia Puig",
            "fechaNacimiento": "15/05/1990",
            "telefono": "612345678",
            "email": "joan@example.com"
          }
        ]
        """;

    [Fact]
    public void ValidJsonArray_ReturnsCustomers()
    {
        var result = JsonCustomerImporter.Import(ValidJson);

        Assert.Empty(result.Errors);
        Assert.Single(result.Customers);
        Assert.Equal(1, result.TotalRows);
        var c = result.Customers[0];
        Assert.Equal("12345678A", c.Dni);
        Assert.Equal("Joan", c.Nombre);
        Assert.Equal("Garcia Puig", c.Apellidos);
        Assert.Equal("15/05/1990", c.FechaNacimiento);
        Assert.Equal("612345678", c.Telefono);
        Assert.Equal("joan@example.com", c.Email);
    }

    [Fact]
    public void JsonWithInvalidRecords_ReportsErrorsWithOriginalData()
    {
        var json = """
            [
              {
                "dni": "12345678A",
                "nombre": "Joan",
                "apellidos": "Garcia Puig",
                "fechaNacimiento": "15/05/1990",
                "telefono": "612345678",
                "email": "joan@example.com"
              },
              {
                "dni": "",
                "nombre": "",
                "apellidos": "",
                "fechaNacimiento": "",
                "telefono": "",
                "email": ""
              }
            ]
            """;

        var result = JsonCustomerImporter.Import(json);

        Assert.Single(result.Customers);
        Assert.Equal(2, result.TotalRows);
        Assert.Single(result.Errors);

        var error = result.Errors[0];
        Assert.Equal(2, error.Row);
        Assert.NotEmpty(error.RawData);
        Assert.NotEmpty(error.Messages);
    }

    [Fact]
    public void EmptyJsonArray_ReturnsEmptyList()
    {
        var result = JsonCustomerImporter.Import("[]");

        Assert.Empty(result.Customers);
        Assert.Empty(result.Errors);
        Assert.Equal(0, result.TotalRows);
    }

    [Fact]
    public void MalformedJson_ReturnsError()
    {
        var result = JsonCustomerImporter.Import("esto no es json");

        Assert.Empty(result.Customers);
        Assert.Single(result.Errors);
        Assert.NotEmpty(result.Errors[0].Messages);
    }

    [Fact]
    public void JsonPreservesOrderOfSuccessAndFailure()
    {
        var json = """
            [
              { "dni": "12345678A", "nombre": "Joan", "apellidos": "Garcia Puig", "fechaNacimiento": "15/05/1990", "telefono": "612345678", "email": "joan@example.com" },
              { "dni": "", "nombre": "", "apellidos": "", "fechaNacimiento": "", "telefono": "", "email": "" },
              { "dni": "87654321B", "nombre": "Maria", "apellidos": "Lopez Soler", "fechaNacimiento": "22/11/1985", "telefono": "698765432", "email": "maria@example.com" }
            ]
            """;

        var result = JsonCustomerImporter.Import(json);

        Assert.Equal(3, result.TotalRows);
        Assert.Equal(2, result.Customers.Count);
        Assert.Equal("12345678A", result.Customers[0].Dni);
        Assert.Equal("87654321B", result.Customers[1].Dni);
        Assert.Single(result.Errors);
        Assert.Equal(2, result.Errors[0].Row);
    }
}
