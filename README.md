# On-Premise Customer Importer

Solución en C# (.NET 8) compuesta por una aplicación de escritorio (WinForms) para importar y gestionar clientes, y una Web API REST para exponer operaciones CRUD sobre los mismos datos.

## Estructura

```
src/
  CustomerImporter.Core/            ← Lógica de dominio (.NET 8, cross-platform)
  CustomerImporter.Core.Tests/      ← Tests unitarios (xUnit, 99 tests)
  CustomerImporter.Desktop/         ← WinForms (.NET 8)
  CustomerImporter.Api/             ← Web API (Minimal API, .NET 8)
  CustomerImporter.Api.Tests/       ← Tests de integración (xUnit, 18 tests)
ejemplos/
  clientes.csv                      ← Fichero de ejemplo CSV
  clientes.json                     ← Fichero de ejemplo JSON
```

## Enfoque

- **TDD**: todo el código se desarrolló guiado por tests (red-green-refactor). 117 tests en total: 99 unitarios de dominio y 18 de integración de la API.
- **Separación UI / Dominio**: la lógica vive en `CustomerImporter.Core` (testable en cualquier plataforma). Desktop y API son capas finas que orquestan servicios.
- **Almacenamiento**: `Dictionary<string, Customer>` en memoria, persistido en `data/clientes_store.db` (JSON serializado). Sin base de datos externa.

## Web API

ASP.NET Core Minimal API que expone operaciones CRUD sobre clientes almacenados en fichero.

| Verbo | Ruta | Respuesta OK | Errores |
|-------|------|--------------|---------|
| GET | `/clientes` | 200 + lista | — |
| GET | `/clientes/{dni}` | 200 + cliente | 404 si no existe |
| POST | `/clientes` | 201 + cliente + `Location` | 400 con errores por campo |
| DELETE | `/clientes/{dni}` | 204 | 404 si no existe |

- Respuestas JSON en camelCase (`dni`, `nombre`, `fechaNacimiento`, ...).
- Errores de validación con detalle por campo: `{ "errors": [{ "field": "Dni", "message": "..." }] }`.
- Persistencia en `data/clientes_store.db` (independiente del Desktop).

## Funcionalidades Desktop

- Importación de CSV y JSON con diálogo de previsualización (ENTRA / NO ENTRA).
- El usuario confirma o cancela la importación tras revisar los resultados.
- DataGridView con edición inline y validación por campo en tiempo real.
- Celdas inválidas marcadas en rosa con tooltip descriptivo.
- Auto-guardado cuando toda la grid es válida. Indicador "Guardado" / "Sin guardar".
- Añadir y eliminar clientes desde la UI.
- Persistencia automática al cerrar. Carga automática al iniciar.

## Validaciones

- **DNI**: 8 dígitos + 1 letra.
- **Nombre / Apellidos**: solo letras (con acentos, ñ, ç), espacios, guiones, apóstrofos.
- **Fecha de nacimiento**: formato dd/MM/yyyy, fecha real, no futura.
- **Teléfono**: 9 dígitos o +XX seguido de 9 dígitos.
- **Email**: formato usuario@dominio.ext.

## Documentación

- [`DECISIONS.md`](DECISIONS.md) — Registro de decisiones técnicas (ADR-001 a ADR-009).
- [`TESTS.md`](TESTS.md) — Especificación de tests unitarios y de integración.

## Ejecución

```bash
# Tests unitarios (dominio)
dotnet test src/CustomerImporter.Core.Tests

# Tests de integración (API)
dotnet test src/CustomerImporter.Api.Tests

# Todos los tests
dotnet test

# Arrancar la API en desarrollo
dotnet run --project src/CustomerImporter.Api

# Publicar Desktop (self-contained, Windows ARM64)
dotnet publish src/CustomerImporter.Desktop -c Release -r win-arm64 --self-contained true -o publish/win-arm64
```
