using CustomerImporter.Core.Models;
using CustomerImporter.Core.Services;

namespace CustomerImporter.Core.Tests;

public class CsvCustomerExporterTests
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
    public void Export_EmptyList_ReturnsOnlyHeader()
    {
        var csv = CsvCustomerExporter.Export([]);

        Assert.Equal("dni,nombre,apellidos,fechaNacimiento,telefono,email", csv.TrimEnd());
    }

    [Fact]
    public void Export_SingleCustomer_ReturnsHeaderAndRow()
    {
        var csv = CsvCustomerExporter.Export([MakeCustomer()]);
        var lines = csv.Split('\n', StringSplitOptions.RemoveEmptyEntries);

        Assert.Equal(2, lines.Length);
        Assert.Equal("dni,nombre,apellidos,fechaNacimiento,telefono,email", lines[0]);
        Assert.Equal("12345678A,Joan,Garcia Puig,15/05/1990,612345678,joan@example.com", lines[1]);
    }

    [Fact]
    public void Export_MultipleCustomers_PreservesOrder()
    {
        var customers = new List<Customer>
        {
            MakeCustomer("12345678A"),
            MakeCustomer("87654321B")
        };

        var csv = CsvCustomerExporter.Export(customers);
        var lines = csv.Split('\n', StringSplitOptions.RemoveEmptyEntries);

        Assert.Equal(3, lines.Length);
        Assert.StartsWith("12345678A", lines[1]);
        Assert.StartsWith("87654321B", lines[2]);
    }

    [Fact]
    public void Export_Roundtrip_CsvImportRecoversData()
    {
        var original = new List<Customer> { MakeCustomer() };

        var csv = CsvCustomerExporter.Export(original);
        var result = CsvCustomerImporter.Import(csv);

        Assert.Single(result.Customers);
        Assert.Equal("12345678A", result.Customers[0].Dni);
        Assert.Equal("Joan", result.Customers[0].Nombre);
    }
}
