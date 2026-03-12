# On-Premise Customer Importer

Aplicación de escritorio (WinForms, .NET 8) para importar clientes desde ficheros CSV o JSON, visualizarlos en una rejilla y persistirlos localmente.

## Estructura

```
src/
  CustomerImporter.Core/            ← Lógica de dominio (.NET 8, cross-platform)
  CustomerImporter.Core.Tests/      ← Tests unitarios (xUnit, 91 tests)
  CustomerImporter.Desktop/         ← WinForms (.NET 8)
  CustomerImporter.Api/             ← Web API (Minimal API, .NET 8)
ejemplos/
  clientes.csv                      ← Fichero de ejemplo CSV
  clientes.json                     ← Fichero de ejemplo JSON
```

## Enfoque

- **TDD**: toda la lógica de dominio se desarrolló guiada por tests (red-green-refactor). 91 tests unitarios cubren validación, importación CSV/JSON y persistencia.
- **Separación UI / Dominio**: la lógica vive en `CustomerImporter.Core` (testable en cualquier plataforma). `MainForm` es una capa fina que orquesta servicios.
- **Almacenamiento**: `Dictionary<string, Customer>` en memoria, persistido en `data/clientes_store.db` (JSON serializado). Sin base de datos externa.

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

- [`DECISIONS.md`](DECISIONS.md) — Registro de decisiones técnicas (ADR-001 a ADR-007).
- [`TESTS.md`](TESTS.md) — Especificación de tests unitarios.

## Ejecución

```bash
# Tests
dotnet test src/CustomerImporter.Core.Tests

# Publicar Desktop (self-contained, Windows ARM64)
dotnet publish src/CustomerImporter.Desktop -c Release -r win-arm64 --self-contained true -o publish/win-arm64
```
