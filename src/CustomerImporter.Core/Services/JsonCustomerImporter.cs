using CustomerImporter.Core.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace CustomerImporter.Core.Services;

public static class JsonCustomerImporter
{
    public static ImportResult Import(string jsonContent)
    {
        var result = new ImportResult();

        JArray rawArray;
        try
        {
            rawArray = JArray.Parse(jsonContent);
        }
        catch (JsonException ex)
        {
            result.Errors.Add(new ImportError
            {
                Row = 0,
                RawData = jsonContent.Length > 200 ? jsonContent[..200] + "..." : jsonContent,
                Messages = [$"JSON mal formado: {ex.Message}"]
            });
            return result;
        }

        result.TotalRows = rawArray.Count;

        for (int i = 0; i < rawArray.Count; i++)
        {
            var token = rawArray[i];
            var rawData = token.ToString(Formatting.None);

            Customer? customer;
            try
            {
                customer = token.ToObject<Customer>();
            }
            catch
            {
                result.Errors.Add(new ImportError
                {
                    Row = i + 1,
                    RawData = rawData,
                    Messages = ["No se pudo deserializar el registro."]
                });
                continue;
            }

            if (customer is null)
            {
                result.Errors.Add(new ImportError
                {
                    Row = i + 1,
                    RawData = rawData,
                    Messages = ["Registro nulo."]
                });
                continue;
            }

            var errors = Validate(customer);
            if (errors.Count > 0)
            {
                result.Errors.Add(new ImportError
                {
                    Row = i + 1,
                    RawData = rawData,
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
