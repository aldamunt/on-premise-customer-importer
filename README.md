# On-Premise Customer Importer

Aplicación de escritorio (WinForms, .NET 8) para importar clientes desde ficheros CSV o JSON, visualizarlos en una rejilla y persistirlos localmente.

## Estructura

```
src/
  CustomerImporter.Desktop/     ← WinForms (.NET 8)
  CustomerImporter.Api/         ← Web API (Minimal API, .NET 8)
ejemplos/
  clientes.csv                  ← Fichero de ejemplo CSV
  clientes.json                 ← Fichero de ejemplo JSON
```

## Documentación

- [`DECISIONS.md`](DECISIONS.md) — Registro de decisiones técnicas (ADR).
- [`TESTS.md`](TESTS.md) — Especificación de tests unitarios.
