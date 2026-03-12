using CustomerImporter.Core.Services;

namespace CustomerImporter.Core.Tests;

public class CustomerValidatorDniTests
{
    [Fact]
    public void Dni_Null_ReturnsFalse() =>
        Assert.False(CustomerValidator.IsValidDni(null));

    [Fact]
    public void Dni_Empty_ReturnsFalse() =>
        Assert.False(CustomerValidator.IsValidDni(""));

    [Fact]
    public void Dni_7DigitsAndLetter_ReturnsFalse() =>
        Assert.False(CustomerValidator.IsValidDni("1234567X"));

    [Fact]
    public void Dni_10DigitsAndLetter_ReturnsFalse() =>
        Assert.False(CustomerValidator.IsValidDni("123456789X"));

    [Fact]
    public void Dni_8DigitsNoLetter_ReturnsFalse() =>
        Assert.False(CustomerValidator.IsValidDni("12345678"));

    [Fact]
    public void Dni_8DigitsAndLetter_ReturnsTrue() =>
        Assert.True(CustomerValidator.IsValidDni("12345678X"));
}

public class CustomerValidatorNameTests
{
    [Fact]
    public void Name_Null_ReturnsFalse() =>
        Assert.False(CustomerValidator.IsValidName(null));

    [Fact]
    public void Name_Empty_ReturnsFalse() =>
        Assert.False(CustomerValidator.IsValidName(""));

    [Theory]
    [InlineData("Carlos123")]
    [InlineData("Test<Name")]
    [InlineData("Test>Name")]
    [InlineData("Test&Name")]
    [InlineData("Test\"Name")]
    [InlineData("Test/Name")]
    [InlineData("Test\\Name")]
    [InlineData("Test|Name")]
    [InlineData("Test;Name")]
    [InlineData("Test:Name")]
    [InlineData("Test!Name")]
    [InlineData("Test?Name")]
    [InlineData("Test@Name")]
    [InlineData("Test#Name")]
    [InlineData("Test$Name")]
    [InlineData("Test%Name")]
    [InlineData("Test^Name")]
    [InlineData("Test*Name")]
    [InlineData("Test(Name")]
    [InlineData("Test)Name")]
    [InlineData("Test[Name")]
    [InlineData("Test]Name")]
    [InlineData("Test{Name")]
    [InlineData("Test}Name")]
    [InlineData("Test=Name")]
    [InlineData("Test+Name")]
    [InlineData("Test~Name")]
    public void Name_InvalidCharacters_ReturnsFalse(string name) =>
        Assert.False(CustomerValidator.IsValidName(name));

    [Theory]
    [InlineData("Carlos")]
    [InlineData("José")]
    [InlineData("Núñez")]
    [InlineData("Açores")]
    [InlineData("De la Cruz")]
    [InlineData("María-José")]
    [InlineData("O'Brien")]
    public void Name_ValidFormats_ReturnsTrue(string name) =>
        Assert.True(CustomerValidator.IsValidName(name));
}

public class CustomerValidatorFechaNacimientoTests
{
    [Fact]
    public void Fecha_Null_ReturnsFalse() =>
        Assert.False(CustomerValidator.IsValidFechaNacimiento(null));

    [Fact]
    public void Fecha_Empty_ReturnsFalse() =>
        Assert.False(CustomerValidator.IsValidFechaNacimiento(""));

    [Fact]
    public void Fecha_Future_ReturnsFalse() =>
        Assert.False(CustomerValidator.IsValidFechaNacimiento("15/06/2099"));

    [Fact]
    public void Fecha_Impossible_ReturnsFalse() =>
        Assert.False(CustomerValidator.IsValidFechaNacimiento("30/02/2000"));

    [Fact]
    public void Fecha_TwoDigitYear_ReturnsFalse() =>
        Assert.False(CustomerValidator.IsValidFechaNacimiento("15/06/95"));

    [Fact]
    public void Fecha_DashSeparator_ReturnsFalse() =>
        Assert.False(CustomerValidator.IsValidFechaNacimiento("15-06-1990"));

    [Fact]
    public void Fecha_DotSeparator_ReturnsFalse() =>
        Assert.False(CustomerValidator.IsValidFechaNacimiento("15.06.1990"));

    [Fact]
    public void Fecha_InvertedFormat_ReturnsFalse() =>
        Assert.False(CustomerValidator.IsValidFechaNacimiento("1990/06/15"));

    [Fact]
    public void Fecha_ValidFormat_ReturnsTrue() =>
        Assert.True(CustomerValidator.IsValidFechaNacimiento("15/06/1990"));
}

public class CustomerValidatorTelefonoTests
{
    [Fact]
    public void Telefono_Null_ReturnsFalse() =>
        Assert.False(CustomerValidator.IsValidTelefono(null));

    [Fact]
    public void Telefono_Empty_ReturnsFalse() =>
        Assert.False(CustomerValidator.IsValidTelefono(""));

    [Fact]
    public void Telefono_LessThan9Digits_ReturnsFalse() =>
        Assert.False(CustomerValidator.IsValidTelefono("12345678"));

    [Fact]
    public void Telefono_ContainsLetters_ReturnsFalse() =>
        Assert.False(CustomerValidator.IsValidTelefono("61234567A"));

    [Fact]
    public void Telefono_ContainsSpaces_ReturnsFalse() =>
        Assert.False(CustomerValidator.IsValidTelefono("612 345 678"));

    [Fact]
    public void Telefono_ContainsDashes_ReturnsFalse() =>
        Assert.False(CustomerValidator.IsValidTelefono("612-345-678"));

    [Fact]
    public void Telefono_9Digits_ReturnsTrue() =>
        Assert.True(CustomerValidator.IsValidTelefono("612345678"));

    [Fact]
    public void Telefono_WithPlusPrefix_ReturnsTrue() =>
        Assert.True(CustomerValidator.IsValidTelefono("+34612345678"));

    [Fact]
    public void Telefono_PrefixWithoutPlus_ReturnsFalse() =>
        Assert.False(CustomerValidator.IsValidTelefono("34612345678"));
}

public class CustomerValidatorEmailTests
{
    [Fact]
    public void Email_Null_ReturnsFalse() =>
        Assert.False(CustomerValidator.IsValidEmail(null));

    [Fact]
    public void Email_Empty_ReturnsFalse() =>
        Assert.False(CustomerValidator.IsValidEmail(""));

    [Fact]
    public void Email_NoAt_ReturnsFalse() =>
        Assert.False(CustomerValidator.IsValidEmail("usuariodominio.com"));

    [Fact]
    public void Email_NoDotAfterAt_ReturnsFalse() =>
        Assert.False(CustomerValidator.IsValidEmail("usuario@dominio"));

    [Fact]
    public void Email_Valid_ReturnsTrue() =>
        Assert.True(CustomerValidator.IsValidEmail("usuario@dominio.com"));

    [Fact]
    public void Email_ValidWithDots_ReturnsTrue() =>
        Assert.True(CustomerValidator.IsValidEmail("usuario.nombre@dominio.es"));
}
