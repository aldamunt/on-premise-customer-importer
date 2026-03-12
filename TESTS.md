# Test Specification

Tests unitarios del proyecto **CustomerImporter.Desktop**, organizados por servicio.

---

## CustomerValidator

### DNI

| Input | Esperado |
|-------|----------|
| `null` | `false` |
| `""` | `false` |
| `1234567X` (7 dígitos + letra) | `false` |
| `123456789X` (10 dígitos + letra) | `false` |
| `12345678` (8 dígitos sin letra) | `false` |
| `12345678X` | `true` |

### Nombre / Apellidos

| Input | Esperado |
|-------|----------|
| `null` | `false` |
| `""` | `false` |
| Contiene `< > & " / \ \| ; : ! ? @ # $ % ^ * ( ) [ ] { } = + ~ 0-9` | `false` |
| `Carlos` | `true` |
| `José` (acento) | `true` |
| `Núñez` (ñ) | `true` |
| `Açores` (ç) | `true` |
| `De la Cruz` (espacio) | `true` |
| `María-José` (guión) | `true` |
| `O'Brien` (apóstrofo) | `true` |

### Fecha de nacimiento

| Input | Esperado |
|-------|----------|
| `null` | `false` |
| `""` | `false` |
| `15/06/2099` (fecha futura) | `false` |
| `30/02/2000` (fecha imposible) | `false` |
| `15/06/95` (año 2 dígitos) | `false` |
| `15-06-1990` (separador incorrecto) | `false` |
| `15.06.1990` (separador incorrecto) | `false` |
| `1990/06/15` (formato invertido) | `false` |
| `15/06/1990` | `true` |

### Teléfono

| Input | Esperado |
|-------|----------|
| `null` | `false` |
| `""` | `false` |
| `12345678` (menos de 9 dígitos) | `false` |
| `61234567A` (contiene letras) | `false` |
| `612 345 678` (contiene espacios) | `false` |
| `612-345-678` (contiene guiones) | `false` |
| `612345678` (9 dígitos) | `true` |
| `+34612345678` (con prefijo) | `true` |
| `34612345678` (prefijo sin +) | `false` |

### Email

| Input | Esperado |
|-------|----------|
| `null` | `false` |
| `""` | `false` |
| `usuariodominio.com` (sin @) | `false` |
| `usuario@dominio` (sin punto después) | `false` |
| `usuario@dominio.com` | `true` |
| `usuario.nombre@dominio.es` | `true` |

---

## CsvCustomerImporter

| Caso | Esperado |
|------|----------|
| CSV válido con cabecera | Lista de clientes correcta |
| CSV con filas inválidas | Reporta errores, no se detiene |
| CSV vacío (solo cabecera) | Lista vacía |
| CSV sin cabecera o mal formado | Error descriptivo |

---

## JsonCustomerImporter

| Caso | Esperado |
|------|----------|
| JSON array válido | Lista de clientes correcta |
| JSON con registros inválidos | Reporta errores, no se detiene |
| JSON vacío (`[]`) | Lista vacía |
| JSON mal formado | Error descriptivo |

---

## CustomerStore

| Caso | Esperado |
|------|----------|
| Guardar lista y volver a cargar | Datos idénticos |
| Cargar cuando no existe fichero | Lista vacía |
| Guardar en directorio inexistente | Crea el directorio |
| Merge: DNI nuevo | Se añade |
| Merge: DNI existente | Se actualiza |
