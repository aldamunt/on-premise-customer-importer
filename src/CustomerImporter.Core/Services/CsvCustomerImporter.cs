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
            result.Errors.Add(new ImportError { Row = 0, RawData = "", Messages = ["El fichero CSV está vacío."] });
            return result;
        }

        var headers = lines[0].Split(',').Select(h => h.Trim()).ToArray();
        if (!ExpectedHeaders.SequenceEqual(headers))
        {
            result.Errors.Add(new ImportError
            {
                Row = 1,
                RawData = lines[0],
                Messages = [$"Cabecera CSV inválida. Se esperaba: {string.Join(",", ExpectedHeaders)}"]
            });
            return result;
        }

        result.TotalRows = lines.Length - 1;

        for (int i = 1; i < lines.Length; i++)
        {
            var rawLine = lines[i];
            var fields = rawLine.Split(',');

            if (fields.Length != 6)
            {
                result.Errors.Add(new ImportError
                {
                    Row = i + 1,
                    RawData = rawLine,
                    Messages = [$"Número de columnas incorrecto ({fields.Length})."]
                });
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

            var errors = Validate(customer);
            if (errors.Count > 0)
            {
                result.Errors.Add(new ImportError
                {
                    Row = i + 1,
                    RawData = rawLine,
                    Messages = errors
                });
                continue;
            }

            result.Customers.Add(customer);
        }

        return result;
    }

    private static List<string> Validate(Customer c)
    {
        var errors = new List<string>();

        if (!CustomerValidator.IsValidDni(c.Dni))
            errors.Add($"DNI inválido '{c.Dni}'.");
        if (!CustomerValidator.IsValidName(c.Nombre))
            errors.Add($"Nombre inválido '{c.Nombre}'.");
        if (!CustomerValidator.IsValidName(c.Apellidos))
            errors.Add($"Apellidos inválidos '{c.Apellidos}'.");
        if (!CustomerValidator.IsValidFechaNacimiento(c.FechaNacimiento))
            errors.Add($"Fecha de nacimiento inválida '{c.FechaNacimiento}'.");
        if (!CustomerValidator.IsValidTelefono(c.Telefono))
            errors.Add($"Teléfono inválido '{c.Telefono}'.");
        if (!CustomerValidator.IsValidEmail(c.Email))
            errors.Add($"Email inválido '{c.Email}'.");

        return errors;
    }
}
