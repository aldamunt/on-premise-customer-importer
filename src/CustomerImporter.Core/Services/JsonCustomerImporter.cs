using CustomerImporter.Core.Models;
using Newtonsoft.Json;

namespace CustomerImporter.Core.Services;

public static class JsonCustomerImporter
{
    public static ImportResult Import(string jsonContent)
    {
        var result = new ImportResult();

        List<Customer>? raw;
        try
        {
            raw = JsonConvert.DeserializeObject<List<Customer>>(jsonContent);
        }
        catch (JsonException ex)
        {
            result.Errors.Add($"JSON mal formado: {ex.Message}");
            return result;
        }

        if (raw is null || raw.Count == 0)
            return result;

        for (int i = 0; i < raw.Count; i++)
        {
            var errors = Validate(raw[i], i + 1);
            if (errors.Count > 0)
            {
                result.Errors.AddRange(errors);
                continue;
            }

            result.Customers.Add(raw[i]);
        }

        return result;
    }

    private static List<string> Validate(Customer c, int index)
    {
        var errors = new List<string>();

        if (!CustomerValidator.IsValidDni(c.Dni))
            errors.Add($"Registro {index}: DNI inválido '{c.Dni}'.");
        if (!CustomerValidator.IsValidName(c.Nombre))
            errors.Add($"Registro {index}: nombre inválido '{c.Nombre}'.");
        if (!CustomerValidator.IsValidName(c.Apellidos))
            errors.Add($"Registro {index}: apellidos inválidos '{c.Apellidos}'.");
        if (!CustomerValidator.IsValidFechaNacimiento(c.FechaNacimiento))
            errors.Add($"Registro {index}: fecha de nacimiento inválida '{c.FechaNacimiento}'.");
        if (!CustomerValidator.IsValidTelefono(c.Telefono))
            errors.Add($"Registro {index}: teléfono inválido '{c.Telefono}'.");
        if (!CustomerValidator.IsValidEmail(c.Email))
            errors.Add($"Registro {index}: email inválido '{c.Email}'.");

        return errors;
    }
}
