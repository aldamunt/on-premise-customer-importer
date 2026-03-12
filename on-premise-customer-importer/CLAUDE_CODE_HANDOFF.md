# Handoff: On-Premise Customer Importer → Claude Code

## Repositori

- **Remote**: `https://github.com/aldamunt/on-premise-customer-importer`
- **Branca**: `main` (up to date, clean working tree)
- **Path local**: `/Users/tomate/Dev/on-premise-customer-importer/on-premise-customer-importer/on-premise-customer-importer`
- **.NET SDK**: 8.0, instal·lat a `$HOME/.dotnet` (no global). PATH configurat a `.zshrc`.

---

## Estructura del projecte

```
.
├── DECISIONS.md              ← Registre de decisions (ADR-001 a ADR-008)
├── TESTS.md                  ← Especificació de tests unitaris
├── README.md                 ← Documentació general
├── OnPremiseCustomerImporter.sln
├── ejemplos/
│   ├── clientes.csv
│   └── clientes.json
├── publish/                  ← Binaris WinForms (gitignored)
└── src/
    ├── CustomerImporter.Core/            ← Lògica de domini (net8.0, cross-platform)
    │   ├── Models/
    │   │   ├── Customer.cs               ← DTO: Dni, Nombre, Apellidos, FechaNacimiento, Telefono, Email
    │   │   ├── ImportResult.cs           ← Customers + Errors + TotalRows
    │   │   ├── ImportError.cs            ← Row, RawData, Messages
    │   │   └── ValidationError.cs        ← Field, Message
    │   └── Services/
    │       ├── CustomerValidator.cs      ← Validació estàtica per camp + Validate(Customer) → List<ValidationError>
    │       ├── CsvCustomerImporter.cs    ← Import(string csv) → ImportResult
    │       ├── JsonCustomerImporter.cs   ← Import(string json) → ImportResult
    │       ├── CustomerStore.cs          ← Load/Save/Merge amb Dictionary<string,Customer> en fitxer .db (JSON)
    │       ├── CsvCustomerExporter.cs    ← Export(List<Customer>) → string csv
    │       └── JsonCustomerExporter.cs   ← Export(List<Customer>) → string json (camelCase, indented)
    │
    ├── CustomerImporter.Core.Tests/      ← xUnit, 99 tests (tots GREEN)
    │   ├── CustomerValidatorTests.cs     ← Tests per camp individual
    │   ├── CustomerValidatorFullTests.cs ← Tests de Validate(Customer)
    │   ├── CsvCustomerImporterTests.cs
    │   ├── JsonCustomerImporterTests.cs
    │   ├── CustomerStoreTests.cs
    │   ├── CsvCustomerExporterTests.cs
    │   └── JsonCustomerExporterTests.cs
    │
    ├── CustomerImporter.Desktop/         ← WinForms (net8.0-windows) — COMPLETAT
    │   ├── MainForm.cs                   ← UI principal amb ToolStrip, DataGridView, StatusStrip
    │   ├── ImportDialog.cs               ← Diàleg de previsualització d'importació
    │   └── Program.cs
    │
    └── CustomerImporter.Api/             ← Web API (net8.0) — PENDENT D'IMPLEMENTAR
        ├── CustomerImporter.Api.csproj   ← Scaffold amb Swagger, SENSE referència a Core
        ├── Program.cs                    ← Template WeatherForecast (cal reemplaçar)
        ├── appsettings.json
        └── Properties/launchSettings.json
```

---

## Estat actual

### COMPLETAT (Desktop)
- **99 tests unitaris** passant (xUnit): validació, importació CSV/JSON, exportació CSV/JSON, persistència.
- **WinForms Desktop** funcional: import, export, edició inline, validació reactiva, auto-persistència, diàleg de previsualització.
- **8 decisions documentades** a `DECISIONS.md` (ADR-001 a ADR-008).
- Publicat com a self-contained `win-arm64` a `/Users/tomate/Desktop/CustomerImporter/`.

### PENDENT (Web API)
El projecte `CustomerImporter.Api` existeix com a scaffold (template WeatherForecast). Cal implementar-lo.

---

## Tasca pendent: Web API

### Requisits (del PDF `Prueba Tecnica C Sharp - Requisitos.pdf`)

| Verb   | Ruta               | Resposta OK              | Errors                           |
|--------|--------------------|--------------------------|------------------------------------|
| GET    | `/clientes`        | 200 + `List<Customer>`   | —                                  |
| GET    | `/clientes/{dni}`  | 200 + `Customer`         | 404 si no existeix                 |
| POST   | `/clientes`        | 201 + Customer + Location| 400 si DNI duplicat o dades invàlides |
| DELETE | `/clientes/{dni}`  | 204                      | 404 si no existeix                 |

- No auth, no base de dades. Persistència en fitxer JSON.
- ASP.NET Core Minimal API recomanat.

### Decisions ja preses per l'usuari

1. **TDD amb tests d'integració** usant `WebApplicationFactory<Program>`.
2. **Mateix fitxer de persistència** que el Desktop: `data/clientes_store.db` (compartit).
3. **Errors detallats per camp** al 400 del POST:
   ```json
   {
     "errors": [
       { "field": "Dni", "message": "DNI inválido. Formato: 8 dígitos + letra." }
     ]
   }
   ```
   Si el DNI ja existeix:
   ```json
   {
     "errors": [
       { "field": "Dni", "message": "Ya existe un cliente con DNI 12345678X." }
     ]
   }
   ```

### Pla d'implementació (TDD)

1. **Crear projecte de tests** `src/CustomerImporter.Api.Tests/` amb xUnit + `Microsoft.AspNetCore.Mvc.Testing`.
2. **Escriure tests d'integració (RED)**:
   - GET /clientes → llista buida, llista amb dades
   - GET /clientes/{dni} → 200 existent, 404 inexistent
   - POST /clientes → 201 vàlid, 400 duplicat, 400 invàlid amb errors detallats
   - DELETE /clientes/{dni} → 204 existent, 404 inexistent
   - Roundtrip: POST + GET comprova integritat
3. **Implementar Program.cs (GREEN)**:
   - Afegir `ProjectReference` a `CustomerImporter.Core` al `.csproj`
   - Registrar `CustomerStore` com a singleton al DI
   - 4 endpoints Minimal API
   - Mantenir Swagger/OpenAPI
4. **Thread-safety**: afegir `lock` o `ReaderWriterLockSlim` a les operacions de fitxer (la API rep requests simultànies).
5. **Documentar**: ADR-009 a `DECISIONS.md`, secció API a `TESTS.md` i `README.md`.
6. **Afegir projecte de tests a la solució** (`OnPremiseCustomerImporter.sln`).
7. **Commit i push**.

### Serveis Core reutilitzables directament

- `CustomerStore` → Load/Save/Merge (constructor rep path del fitxer)
- `CustomerValidator.Validate(Customer)` → retorna `List<ValidationError>` per al 400
- `Customer`, `ValidationError` → models

### Detalls tècnics importants

- `CustomerStore.Load()` retorna `Dictionary<string, Customer>` (clau = DNI).
- `CustomerStore.Save(Dictionary<string, Customer>)` serialitza a JSON amb Newtonsoft.Json.
- `CustomerStore.Merge(existing, incoming)` afegeix o actualitza per DNI.
- `CustomerValidator` és `static partial class` amb `GeneratedRegex`.
- El fitxer de persistència és `data/clientes_store.db` (relatiu al executable).
- Al `.csproj` de l'API cal afegir: `<ProjectReference Include="..\CustomerImporter.Core\CustomerImporter.Core.csproj" />`

---

## Comandes útils

```bash
# Executar tests existents (99 tests)
dotnet test src/CustomerImporter.Core.Tests

# Executar API en dev
dotnet run --project src/CustomerImporter.Api

# Publicar Desktop per Windows ARM64
dotnet publish src/CustomerImporter.Desktop -c Release -r win-arm64 --self-contained true -o publish/win-arm64

# Copiar a la VM
cp -r publish/win-arm64/* /Users/tomate/Desktop/CustomerImporter/
```

---

## Estil de codi i convencions

- **Idioma del codi**: C# amb noms de propietats en castellà (Dni, Nombre, Apellidos, FechaNacimiento, Telefono, Email).
- **Idioma dels commits**: castellà.
- **Idioma de la documentació** (DECISIONS.md, README.md, TESTS.md): castellà.
- **Idioma de la conversa amb l'usuari**: català.
- **Testing**: xUnit, un fitxer de test per servei, noms descriptius en anglès.
- **Arquitectura**: tota la lògica a Core (testable), UI/API com a capes fines.
- **Serialització**: Newtonsoft.Json (no System.Text.Json) per compatibilitat amb el Desktop.
- **Format commit**: una línia descriptiva en castellà, sense prefix convencional.

---

## Historial de commits

```
0e63021 Exportación a CSV y JSON con TDD (99 tests green)
40a7853 Validación inline, diálogo de previsualización y UI de gestión (91 tests)
6732ac1 Implementar MainForm WinForms con DataGridView, importación y persistencia
6b723b9 Implementar lógica de dominio con TDD (79 tests green)
29683f4 Ampliar especificación de tests con validaciones detalladas por campo
d1520cf Configuración inicial del proyecto con decisiones y tests documentados
28f0260 Initial commit
```
