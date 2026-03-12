# Decision Log

Decisiones técnicas y de diseño del proyecto **On-Premise Customer Importer**.

---

### ADR-001: Almacenamiento en memoria + fichero
- `Dictionary<string, Customer>` indexado por DNI como almacén principal.
- Persistencia en fichero `.db` (JSON serializado) al cerrar la app.
- Carga automática al iniciar si el fichero existe.
- Sin base de datos externa.

### ADR-002: TDD con separación UI / Dominio
- Toda la lógica de negocio en clases de servicio testables.
- `MainForm` como capa fina: solo orquesta servicios y actualiza UI.
- Desarrollo guiado por tests (red-green-refactor).

### ADR-003: Especificación de tests
- Tests unitarios por servicio: Validator, CsvImporter, JsonImporter, Store.
- Detalle completo en [`TESTS.md`](TESTS.md).

### ADR-004: UI WinForms
- Edición inline activada en la DataGridView.
- Botón para añadir clientes manualmente desde la UI.
- Barra de progreso real (avance por registro procesado) durante la importación.

### ADR-005: Validación inline y persistencia reactiva
- Validación completa del cliente al terminar de editar cualquier celda.
- Celdas inválidas marcadas en rosa con tooltip descriptivo.
- Auto-guardado al disco cuando toda la grid es válida.
- Indicador de estado "Guardado" / "Sin guardar" siempre visible.
- Al cerrar con errores: aviso y opción de salir sin guardar.

### ADR-006: Diálogo de previsualización antes de importar
- Pantalla intermedia que muestra qué registros entrarán (ENTRA) y cuáles no (NO ENTRA) con motivo.
- El usuario decide si confirma ("Importar") o cancela.
- Enriquecido ImportResult con datos originales del registro fallido (ImportError).
- Si todos los registros son inválidos, el botón "Importar" queda desactivado.

### ADR-007: Resolución y tamaño estándar de gestión
- Ventana principal: 1280x720 (mínimo 1024x600).
- Diálogo de importación: 75% de la principal (960x540).
- Grid con filas de 38px, cabeceras de 42px, columnas proporcionales al contenido.
- Fuente Segoe UI 9.5pt, estándar para aplicaciones contables.

### ADR-008: Exportación a CSV y JSON
- El usuario puede exportar los clientes actuales a CSV o JSON.
- SaveFileDialog para elegir carpeta y nombre de fichero.
- Formato CSV con misma cabecera que la importación (roundtrip compatible).
- Formato JSON con camelCase y pretty-print.
- Tests de roundtrip: exportar → importar recupera los datos originales.

---

## Web API

### ADR-009: Arquitectura de la Web API
- ASP.NET Core Minimal API (.NET 8), por simplicidad y alineación con el spec.
- `Program.cs` como capa fina: reutiliza `CustomerStore`, `CustomerValidator` y `Customer` del proyecto Core.
- Fichero de persistencia propio (`data/clientes_store.db`), independiente del Desktop.
- `Dictionary<string, Customer>` interno se mantiene por rendimiento; la API serializa los valores como array en las respuestas.
- Thread-safety con `lock` inline en `Program.cs`, sin wrappers adicionales.
- Ruta del fichero configurable via clave `StorePath` en `IConfiguration`, con valor por defecto `data/clientes_store.db`.

### ADR-010: Contrato HTTP de la API
- Respuestas JSON en camelCase (`dni`, `nombre`, `fechaNacimiento`, ...) via `ConfigureHttpJsonOptions`.
- Errores de validación en el 400 con estructura `{ "errors": [{ "field": "...", "message": "..." }] }`.
- Header `Location` en el 201 apunta a `/clientes/{dni}`.

### ADR-011: Estrategia de tests de integración
- TDD con `WebApplicationFactory<Program>`; `public partial class Program` expone la clase al proyecto de tests.
- Tests aislados: cada clase hereda de `ApiTestBase`, que crea un fichero temporal único y lo elimina al finalizar (`IDisposable`).
- `ApiTestBase` provee helpers `SeedCliente` (escritura directa al store) y `JsonBody` (serialización camelCase para POST).
- Detalle completo en [`TESTS.md`](TESTS.md).

---

*Cada nueva decisión se añade al final con el siguiente número de ADR.*
