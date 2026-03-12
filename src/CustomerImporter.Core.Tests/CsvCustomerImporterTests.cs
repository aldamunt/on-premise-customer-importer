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
    public void CsvWithInvalidRows_ReportsErrorsWithRawData()
    {
        var invalidRow = ",,,,,";
        var csv = $"{ValidHeader}\n{ValidRow}\n{invalidRow}";

        var result = CsvCustomerImporter.Import(csv);

        Assert.Single(result.Customers);
        Assert.Single(result.Errors);
        Assert.Equal(2, result.TotalRows);

        var error = result.Errors[0];
        Assert.Equal(3, error.Row);
        Assert.Equal(invalidRow, error.RawData);
        Assert.NotEmpty(error.Messages);
    }

    [Fact]
    public void CsvWithOnlyHeader_ReturnsEmptyList()
    {
        var csv = ValidHeader;

        var result = CsvCustomerImporter.Import(csv);

        Assert.Empty(result.Customers);
        Assert.Empty(result.Errors);
        Assert.Equal(0, result.TotalRows);
    }

    [Fact]
    public void CsvMalformed_NoHeader_ReturnsError()
    {
        var csv = "esto no es un csv valido";

        var result = CsvCustomerImporter.Import(csv);

        Assert.Empty(result.Customers);
        Assert.Single(result.Errors);
        Assert.NotEmpty(result.Errors[0].Messages);
    }

    [Fact]
    public void CsvWithWrongColumnCount_ReportsRowAndRawData()
    {
        var badRow = "12345678A,Joan,Garcia";
        var csv = $"{ValidHeader}\n{badRow}";

        var result = CsvCustomerImporter.Import(csv);

        Assert.Empty(result.Customers);
        Assert.Single(result.Errors);
        Assert.Equal(2, result.Errors[0].Row);
        Assert.Equal(badRow, result.Errors[0].RawData);
    }

    [Fact]
    public void CsvPreservesOrderOfSuccessAndFailure()
    {
        var row1 = "12345678A,Joan,Garcia Puig,15/05/1990,612345678,joan@example.com";
        var row2 = ",,,,,";
        var row3 = "87654321B,Maria,Lopez Soler,22/11/1985,698765432,maria@example.com";
        var csv = $"{ValidHeader}\n{row1}\n{row2}\n{row3}";

        var result = CsvCustomerImporter.Import(csv);

        Assert.Equal(3, result.TotalRows);
        Assert.Equal(2, result.Customers.Count);
        Assert.Equal("12345678A", result.Customers[0].Dni);
        Assert.Equal("87654321B", result.Customers[1].Dni);
        Assert.Single(result.Errors);
        Assert.Equal(3, result.Errors[0].Row);
    }
}
