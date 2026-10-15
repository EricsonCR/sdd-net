# Modelo de datos: Catálogo de estados y listado paginado

**Iniciativa**: `004-properties-list-pagination`  
**Spec**: [spec.md](spec.md)  
**Plan**: [plan.md](plan.md)

## `PropertyStatus`

Representa un estado admitido para las propiedades.

| Campo | Tipo conceptual | Reglas |
|---|---|---|
| `Value` | Texto | Requerido, clave primaria y único. Comparación exacta y capitalización canónica. |
| `Description` | Texto | Requerido; proviene del archivo de estados declarado por el manifiesto. |

Valores iniciales: `Available`, `Rented` y `Maintenance`, con las descripciones vigentes en `support/seed-data/properties-statuses.json`.

## `Property`

Conserva los atributos persistidos de 003. `Status` permanece como texto con su valor actual y pasa a ser clave foránea obligatoria a `PropertyStatus.Value`. La entidad mantiene una navegación requerida al registro del catálogo; no se reemplazan los datos ni el campo físico de estado.

| Campo existente | Tipo conceptual | Reglas relevantes para 004 |
|---|---|---|
| `Id` | UUID | Identidad de la propiedad; no cambia. |
| `Title` | Texto | Requerido; se usa para ordenar ascendentemente. |
| `Description` | Texto | Se conserva. |
| `Address` | Texto | Se conserva y se incluye en la respuesta pública aprobada. |
| `Price` | Decimal | Se conserva. |
| `Status` | Texto | Requerido; FK a `PropertyStatus.Value`; conserva la columna y los valores existentes. |
| `BedroomCount` | Entero | Se conserva. |
| `BathroomCount` | Entero | Se conserva. |
| `AreaSquareMeters` | Decimal | Se conserva. |
| `ImageUrl` | Texto | Se conserva como dato de ruta/nombre de archivo; la URL pública se arma en respuesta. |
| `CreatedAt` | Fecha y hora con zona | Se conserva. |
| `UpdatedAt` | Fecha y hora con zona nullable | Se conserva. |

## Relación e integridad

- `PropertyStatus` tiene cero o muchas propiedades; cada `Property` referencia exactamente un estado.
- La FK usa `Properties.Status` → `property_status.Value`, ambos textuales y con longitud compatible con la columna existente (`32`).
- La PK de `Value` prohíbe duplicar el código canónico. No se agrega una clave sustituta ni una unicidad secundaria.
- La migración carga las tres claves antes de aplicar la FK. Si existen estados históricos fuera de las tres claves reconocidas, falla de forma explícita antes de confirmar cambios.
- La reversión elimina la FK y el catálogo, pero mantiene las propiedades y sus textos de estado.
- El seeding inserta estados faltantes por clave exacta antes de leer propiedades; no agrega propiedades ya existentes por `Id` y no asigna un fallback de estado.

## Contratos de lectura

La respuesta HTTP no es una entidad persistente.

- **Elemento `items`**: `id` (UUID), `title`, `description`, `address`, `price` (decimal), `status` (texto `PropertyStatus.Value`), `bedroomCount`, `bathroomCount`, `areaSquareMeters` (decimal) e `imageUrl` (URL absoluta).
- **Página**: `items`, `page`, `pageSize`, `totalItems`, `totalPages`, `hasNext` y `hasPrevious`.
- **Error de mapping**: si una propiedad seleccionada no contiene un nombre de archivo utilizable, la página completa responde HTTP 500 con `ProblemDetails`; no se devuelve una colección parcial ni se inventa una URL.
- **Origen de URL**: el host y esquema se toman de la solicitud solo después de validar `AllowedHosts`; los forwarded headers solo se aceptan de proxies explícitamente confiables.
- **Cálculo**: `totalPages = 0` si no hay elementos; de otro modo, redondeo hacia arriba de `totalItems / pageSize`. `hasNext` es verdadero cuando `page < totalPages`; `hasPrevious` es verdadero cuando `page > 1`.
- **Paginación**: valores predeterminados `page=1`, `pageSize=6`; ambos positivos; máximo `pageSize=50`; orden por título ascendente y `Id` ascendente como desempate.
- **Estados**: no se modelan transiciones de estado porque esta iniciativa no cambia estados de propiedades; solo normaliza el catálogo y conserva el valor actual.

## Artefactos relacionados

- Contrato HTTP: [contracts/properties-list.md](contracts/properties-list.md).
- Procedimiento manual de validación: [quickstart.md](quickstart.md).