using System.Text;
using CustomerImporter.Core.Models;

namespace CustomerImporter.Core.Services;

public static class CsvCustomerExporter
{
    private const string Header = "dni,nombre,apellidos,fechaNacimiento,telefono,email";

    public static string Export(List<Customer> customers)
    {
        var sb = new StringBuilder();
        sb.AppendLine(Header);

        foreach (var c in customers)
            sb.AppendLine($"{c.Dni},{c.Nombre},{c.Apellidos},{c.FechaNacimiento},{c.Telefono},{c.Email}");

        return sb.ToString();
    }
}
