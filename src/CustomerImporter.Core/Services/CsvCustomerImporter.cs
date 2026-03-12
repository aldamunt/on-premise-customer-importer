using CustomerImporter.Core.Models;

namespace CustomerImporter.Core.Services;

public static class CsvCustomerImporter
{
    private static readonly string[] ExpectedHeaders =
        ["dni", "nombre", "apellidos", "fechaNacimiento", "telefono", "email"];

    public static ImportResult Import(string csvContent)
    {
        var result = new ImportResult();
        var lines = csvContent.Split('\n', StringSplitOptions.RemoveEmptyEntries);

        if (lines.Length == 0)
        {
            result.Errors.Add("El fichero CSV está vacío.");
            return result;
        }

        var headers = lines[0].Split(',').Select(h => h.Trim()).ToArray();
        if (!ExpectedHeaders.SequenceEqual(headers))
        {
            result.Errors.Add($"Cabecera CSV inválida. Se esperaba: {string.Join(",", ExpectedHeaders)}");
            return result;
        }

        for (int i = 1; i < lines.Length; i++)
        {
            var fields = lines[i].Split(',');
            if (fields.Length != 6)
            {
                result.Errors.Add($"Fila {i + 1}: número de columnas incorrecto ({fields.Length}).");
                continue;
            }

            var customer = new Customer
            {
                Dni = fields[0].Trim(),
                Nombre = fields[1].Trim(),
                Apellidos = fields[2].Trim(),
                FechaNacimiento = fields[3].Trim(),
                Telefono = fields[4].Trim(),
                Email = fields[5].Trim()
            };

            var errors = Validate(customer, i + 1);
            if (errors.Count > 0)
            {
                result.Errors.AddRange(errors);
                continue;
            }

            result.Customers.Add(customer);
        }

        return result;
    }

    private static List<string> Validate(Customer c, int row)
    {
        var errors = new List<string>();

        if (!CustomerValidator.IsValidDni(c.Dni))
            errors.Add($"Fila {row}: DNI inválido '{c.Dni}'.");
        if (!CustomerValidator.IsValidName(c.Nombre))
            errors.Add($"Fila {row}: nombre inválido '{c.Nombre}'.");
        if (!CustomerValidator.IsValidName(c.Apellidos))
            errors.Add($"Fila {row}: apellidos inválidos '{c.Apellidos}'.");
        if (!CustomerValidator.IsValidFechaNacimiento(c.FechaNacimiento))
            errors.Add($"Fila {row}: fecha de nacimiento inválida '{c.FechaNacimiento}'.");
        if (!CustomerValidator.IsValidTelefono(c.Telefono))
            errors.Add($"Fila {row}: teléfono inválido '{c.Telefono}'.");
        if (!CustomerValidator.IsValidEmail(c.Email))
            errors.Add($"Fila {row}: email inválido '{c.Email}'.");

        return errors;
    }
}
