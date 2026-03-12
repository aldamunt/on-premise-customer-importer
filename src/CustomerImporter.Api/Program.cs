using CustomerImporter.Core.Models;
using CustomerImporter.Core.Services;
using System.Text.Json;

var builder = WebApplication.CreateBuilder(args);

// ── Storage ───────────────────────────────────────────────────────────────────
// StorePath can be overridden via configuration (used by integration tests).
// Default: data/clientes_store.db next to the executable.
var storePath = builder.Configuration["StorePath"]
    ?? Path.Combine(AppContext.BaseDirectory, "data", "clientes_store.db");

// Single shared instance — the lock below ensures thread-safe file access
builder.Services.AddSingleton(new CustomerStore(storePath));

// ── JSON serialization ────────────────────────────────────────────────────────
// Use camelCase for all HTTP responses (dni, nombre, fechaNacimiento, ...)
builder.Services.ConfigureHttpJsonOptions(options =>
{
    options.SerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
});

// ── Swagger ───────────────────────────────────────────────────────────────────
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

// ── Thread-safety ─────────────────────────────────────────────────────────────
// All read/write operations on the store file go through this lock.
// This prevents data corruption when multiple requests arrive simultaneously.
var fileLock = new object();

// ── Endpoints ─────────────────────────────────────────────────────────────────

// GET /clientes — returns the full list of customers
app.MapGet("/clientes", (CustomerStore store) =>
{
    lock (fileLock)
    {
        var customers = store.Load();
        return Results.Ok(customers.Values.ToList());
    }
});

// GET /clientes/{dni} — returns one customer by DNI; 404 if not found
app.MapGet("/clientes/{dni}", (string dni, CustomerStore store) =>
{
    lock (fileLock)
    {
        var customers = store.Load();
        if (!customers.TryGetValue(dni, out var customer))
            return Results.NotFound();
        return Results.Ok(customer);
    }
});

// POST /clientes — creates a customer; 400 if data is invalid or DNI already exists
app.MapPost("/clientes", (Customer customer, CustomerStore store) =>
{
    // Validate fields before touching the file
    var errors = CustomerValidator.Validate(customer);
    if (errors.Count > 0)
        return Results.BadRequest(new { errors });

    lock (fileLock)
    {
        var customers = store.Load();

        if (customers.ContainsKey(customer.Dni!))
        {
            var dniError = new ValidationError
            {
                Field = "Dni",
                Message = $"Ya existe un cliente con DNI {customer.Dni}."
            };
            return Results.BadRequest(new { errors = new[] { dniError } });
        }

        customers[customer.Dni!] = customer;
        store.Save(customers);
        return Results.Created($"/clientes/{customer.Dni}", customer);
    }
});

// DELETE /clientes/{dni} — removes a customer; 204 if deleted, 404 if not found
app.MapDelete("/clientes/{dni}", (string dni, CustomerStore store) =>
{
    lock (fileLock)
    {
        var customers = store.Load();

        if (!customers.ContainsKey(dni))
            return Results.NotFound();

        customers.Remove(dni);
        store.Save(customers);
        return Results.NoContent();
    }
});

app.Run();

// Needed so the test project can reference Program via WebApplicationFactory<Program>
public partial class Program { }
