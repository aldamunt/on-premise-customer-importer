using CustomerImporter.Core.Services;

namespace CustomerImporter.Core.Tests;

public class CsvCustomerImporterTests
{
    private const string ValidHeader = "dni,nombre,apellidos,fechaNacimiento,telefono,email";

    private const string ValidRow =
        "12345678A,Joan,Garcia Puig,15/05/1990,612345678,joan@example.com";

    [Fact]
    public void ValidCsvWithHeader_ReturnsCustomers()
    {
        var csv = $"{ValidHeader}\n{ValidRow}";

        var result = CsvCustomerImporter.Import(csv);

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
    public void CsvWithInvalidRows_ReportsErrorsAndContinues()
    {
        var invalidRow = ",,,,,,";
        var csv = $"{ValidHeader}\n{ValidRow}\n{invalidRow}";

        var result = CsvCustomerImporter.Import(csv);

        Assert.Single(result.Customers);
        Assert.NotEmpty(result.Errors);
    }

    [Fact]
    public void CsvWithOnlyHeader_ReturnsEmptyList()
    {
        var csv = ValidHeader;

        var result = CsvCustomerImporter.Import(csv);

        Assert.Empty(result.Customers);
        Assert.Empty(result.Errors);
    }

    [Fact]
    public void CsvMalformed_NoHeader_ReturnsError()
    {
        var csv = "esto no es un csv valido";

        var result = CsvCustomerImporter.Import(csv);

        Assert.Empty(result.Customers);
        Assert.NotEmpty(result.Errors);
    }
}
