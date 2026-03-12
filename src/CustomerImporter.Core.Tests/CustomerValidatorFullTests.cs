using CustomerImporter.Core.Models;
using CustomerImporter.Core.Services;

namespace CustomerImporter.Core.Tests;

public class CustomerValidatorFullTests
{
    private static Customer ValidCustomer() => new()
    {
        Dni = "12345678A",
        Nombre = "Joan",
        Apellidos = "Garcia Puig",
        FechaNacimiento = "15/05/1990",
        Telefono = "612345678",
        Email = "joan@example.com"
    };

    [Fact]
    public void ValidCustomer_ReturnsNoErrors()
    {
        var errors = CustomerValidator.Validate(ValidCustomer());
        Assert.Empty(errors);
    }

    [Fact]
    public void InvalidDni_ReturnsErrorForDni()
    {
        var c = ValidCustomer();
        c.Dni = "";
        var errors = CustomerValidator.Validate(c);
        Assert.Single(errors);
        Assert.Equal("Dni", errors[0].Field);
    }

    [Fact]
    public void InvalidNombre_ReturnsErrorForNombre()
    {
        var c = ValidCustomer();
        c.Nombre = "123";
        var errors = CustomerValidator.Validate(c);
        Assert.Single(errors);
        Assert.Equal("Nombre", errors[0].Field);
    }

    [Fact]
    public void InvalidApellidos_ReturnsErrorForApellidos()
    {
        var c = ValidCustomer();
        c.Apellidos = "";
        var errors = CustomerValidator.Validate(c);
        Assert.Single(errors);
        Assert.Equal("Apellidos", errors[0].Field);
    }

    [Fact]
    public void InvalidFecha_ReturnsErrorForFechaNacimiento()
    {
        var c = ValidCustomer();
        c.FechaNacimiento = "99/99/9999";
        var errors = CustomerValidator.Validate(c);
        Assert.Single(errors);
        Assert.Equal("FechaNacimiento", errors[0].Field);
    }

    [Fact]
    public void InvalidTelefono_ReturnsErrorForTelefono()
    {
        var c = ValidCustomer();
        c.Telefono = "abc";
        var errors = CustomerValidator.Validate(c);
        Assert.Single(errors);
        Assert.Equal("Telefono", errors[0].Field);
    }

    [Fact]
    public void InvalidEmail_ReturnsErrorForEmail()
    {
        var c = ValidCustomer();
        c.Email = "novalid";
        var errors = CustomerValidator.Validate(c);
        Assert.Single(errors);
        Assert.Equal("Email", errors[0].Field);
    }

    [Fact]
    public void MultipleInvalidFields_ReturnsAllErrors()
    {
        var c = new Customer();
        var errors = CustomerValidator.Validate(c);
        Assert.Equal(6, errors.Count);
    }

    [Fact]
    public void ErrorMessages_AreDescriptive()
    {
        var c = ValidCustomer();
        c.Dni = "";
        var errors = CustomerValidator.Validate(c);
        Assert.Contains("DNI", errors[0].Message);
    }
}
