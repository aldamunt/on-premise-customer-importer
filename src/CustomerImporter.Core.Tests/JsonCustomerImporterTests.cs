using CustomerImporter.Core.Services;

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
        var c = result.Customers[0];
        Assert.Equal("12345678A", c.Dni);
        Assert.Equal("Joan", c.Nombre);
        Assert.Equal("Garcia Puig", c.Apellidos);
        Assert.Equal("15/05/1990", c.FechaNacimiento);
        Assert.Equal("612345678", c.Telefono);
        Assert.Equal("joan@example.com", c.Email);
    }

    [Fact]
    public void JsonWithInvalidRecords_ReportsErrorsAndContinues()
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
        Assert.NotEmpty(result.Errors);
    }

    [Fact]
    public void EmptyJsonArray_ReturnsEmptyList()
    {
        var result = JsonCustomerImporter.Import("[]");

        Assert.Empty(result.Customers);
        Assert.Empty(result.Errors);
    }

    [Fact]
    public void MalformedJson_ReturnsError()
    {
        var result = JsonCustomerImporter.Import("esto no es json");

        Assert.Empty(result.Customers);
        Assert.NotEmpty(result.Errors);
    }
}
