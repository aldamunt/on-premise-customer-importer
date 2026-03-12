# Test Specification

Tests unitarios del proyecto **CustomerImporter.Desktop**, organizados por servicio.

---

### CustomerValidator
- [ ] DNI vacío o null → falla
- [ ] Email sin `@` o formato inválido → falla
- [ ] Teléfono con letras o longitud incorrecta → falla
- [ ] Fecha con formato inválido → falla
- [ ] Cliente con todos los campos correctos → pasa

### CsvCustomerImporter
- [ ] CSV válido con cabecera → lista de clientes correcta
- [ ] CSV con filas inválidas → reporta errores, no se detiene
- [ ] CSV vacío (solo cabecera) → lista vacía
- [ ] CSV sin cabecera o mal formado → error descriptivo

### JsonCustomerImporter
- [ ] JSON array válido → lista de clientes correcta
- [ ] JSON con registros inválidos → reporta errores, no se detiene
- [ ] JSON vacío (`[]`) → lista vacía
- [ ] JSON mal formado → error descriptivo

### CustomerStore
- [ ] Guardar lista y volver a cargar → datos idénticos
- [ ] Cargar cuando no existe fichero → lista vacía
- [ ] Guardar en directorio inexistente → crea el directorio
- [ ] Merge: DNI nuevo → se añade
- [ ] Merge: DNI existente → se actualiza
