using CustomerImporter.Core.Models;
using CustomerImporter.Core.Services;

namespace CustomerImporter.Core.Tests;

public class CustomerStoreTests : IDisposable
{
    private readonly string _tempDir;
    private readonly string _filePath;
    private readonly CustomerStore _store;

    public CustomerStoreTests()
    {
        _tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        _filePath = Path.Combine(_tempDir, "test_store.db");
        _store = new CustomerStore(_filePath);
    }

    public void Dispose()
    {
        if (Directory.Exists(_tempDir))
            Directory.Delete(_tempDir, true);
    }

    private static Customer MakeCustomer(string dni) => new()
    {
        Dni = dni,
        Nombre = "Test",
        Apellidos = "User",
        FechaNacimiento = "01/01/2000",
        Telefono = "612345678",
        Email = "test@example.com"
    };

    [Fact]
    public void SaveAndLoad_DataIsIdentical()
    {
        var customers = new Dictionary<string, Customer>
        {
            ["12345678A"] = MakeCustomer("12345678A")
        };

        _store.Save(customers);
        var loaded = _store.Load();

        Assert.Single(loaded);
        Assert.Equal("12345678A", loaded["12345678A"].Dni);
        Assert.Equal("Test", loaded["12345678A"].Nombre);
    }

    [Fact]
    public void Load_FileDoesNotExist_ReturnsEmpty()
    {
        var loaded = _store.Load();

        Assert.Empty(loaded);
    }

    [Fact]
    public void Save_DirectoryDoesNotExist_CreatesIt()
    {
        var customers = new Dictionary<string, Customer>
        {
            ["12345678A"] = MakeCustomer("12345678A")
        };

        _store.Save(customers);

        Assert.True(Directory.Exists(_tempDir));
        Assert.True(File.Exists(_filePath));
    }

    [Fact]
    public void Merge_NewDni_AddsToDict()
    {
        var existing = new Dictionary<string, Customer>
        {
            ["12345678A"] = MakeCustomer("12345678A")
        };
        var incoming = new List<Customer> { MakeCustomer("87654321B") };

        var merged = _store.Merge(existing, incoming);

        Assert.Equal(2, merged.Count);
        Assert.True(merged.ContainsKey("87654321B"));
    }

    [Fact]
    public void Merge_ExistingDni_UpdatesRecord()
    {
        var existing = new Dictionary<string, Customer>
        {
            ["12345678A"] = MakeCustomer("12345678A")
        };
        var updated = MakeCustomer("12345678A");
        updated.Nombre = "Updated";

        var merged = _store.Merge(existing, [updated]);

        Assert.Single(merged);
        Assert.Equal("Updated", merged["12345678A"].Nombre);
    }
}
