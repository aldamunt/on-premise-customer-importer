using System.Globalization;
using System.Text.RegularExpressions;

namespace CustomerImporter.Core.Services;

public static partial class CustomerValidator
{
    public static bool IsValidDni(string? dni)
    {
        if (string.IsNullOrEmpty(dni)) return false;
        return DniRegex().IsMatch(dni);
    }

    public static bool IsValidName(string? name)
    {
        if (string.IsNullOrEmpty(name)) return false;
        return NameRegex().IsMatch(name);
    }

    public static bool IsValidFechaNacimiento(string? fecha)
    {
        if (string.IsNullOrEmpty(fecha)) return false;
        if (!FechaRegex().IsMatch(fecha)) return false;

        if (!DateTime.TryParseExact(fecha, "dd/MM/yyyy",
            CultureInfo.InvariantCulture, DateTimeStyles.None, out var date))
            return false;

        return date <= DateTime.Today;
    }

    public static bool IsValidTelefono(string? telefono)
    {
        if (string.IsNullOrEmpty(telefono)) return false;
        return TelefonoRegex().IsMatch(telefono);
    }

    public static bool IsValidEmail(string? email)
    {
        if (string.IsNullOrEmpty(email)) return false;
        return EmailRegex().IsMatch(email);
    }

    [GeneratedRegex(@"^\d{8}[A-Za-z]$")]
    private static partial Regex DniRegex();

    [GeneratedRegex(@"^[\p{L} '\-]+$")]
    private static partial Regex NameRegex();

    [GeneratedRegex(@"^\d{2}/\d{2}/\d{4}$")]
    private static partial Regex FechaRegex();

    [GeneratedRegex(@"^(\+\d{2,3})?\d{9}$")]
    private static partial Regex TelefonoRegex();

    [GeneratedRegex(@"^[^@\s]+@[^@\s]+\.[^@\s]+$")]
    private static partial Regex EmailRegex();
}
