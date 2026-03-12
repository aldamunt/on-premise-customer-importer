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

---

*Cada nueva decisión se añade al final con el siguiente número de ADR.*
