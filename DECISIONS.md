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

---

*Cada nueva decisión se añade al final con el siguiente número de ADR.*
