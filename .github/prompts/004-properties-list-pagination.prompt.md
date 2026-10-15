---
name: 004-properties-list-pagination
agent: speckit.specify
---

/speckit.specify

Crear o actualizar únicamente `specs/004-properties-list-pagination/spec.md`.

Antes de escribirla, confirma que la iniciativa 003 esté implementada y que no exista una iniciativa 004 con otro nombre. Si esta iniciativa ya existe, actualiza `spec.md` solo si su estado es `Borrador`; en cualquier otro estado, no sobrescribas archivos: informa y detente. Respeta la ubicación canónica de specs y lee la constitución, las instrucciones aplicables y los artefactos relevantes de la iniciativa 003.

Esta ejecución de `speckit.specify` genera únicamente `spec.md`; no generes `plan.md`, `tasks.md`, salidas auxiliares ni código. Las siguientes fases se ejecutarán por separado y derivarán su alcance de la spec aprobada.

## Objetivo
Especificar en una sola iniciativa los siguientes cambios relacionados con propiedades, en orden de dependencia:

1. Persistir `PropertyStatus` como catálogo relacional en una tabla PostgreSQL `property_status`, reemplazando su almacenamiento exclusivo como enum convertido a texto en `Properties.Status`.
2. Crear un endpoint Minimal API basado en ISlice para devolver la lista paginada de propiedades, incluyendo la imagen correspondiente mediante URL absoluta pública.

La spec debe definir el modelo y la migración para relacionar `Property` con el catálogo, preservar los datos existentes y permitir que el endpoint devuelva el estado en el contrato esperado.

## Contexto obligatorio
- Respetar la constitución, las instrucciones de backend y persistencia y las convenciones vigentes del repositorio.
- Seguir skills oficiales aplicables:
  - minimal-api-slices
  - minimal-api-validation
  - vertical-slice-handlers
  - vertical-slice-mapping
  - result-problem-details
- Mantener consistencia con el patrón ISlice y auto-descubrimiento de endpoints.
- No reabrir ni modificar la iniciativa 003; todos los cambios nuevos pertenecen a la 004.
- Revisar el modelo EF Core, la configuración de `Property`, las migraciones, el seeder y los archivos de seed para documentar el estado actual antes de especificar cambios.
- Si falta una decisión de producto o de modelo que impida fijar requisitos verificables, formular las preguntas interactivas requeridas por la constitución antes de cerrar la spec; no inventar decisiones silenciosamente.
- No introducir controllers.

## Estructura obligatoria
La lógica del endpoint debe vivir en Features dentro de RealtorApi:

app/backend/src/RealtorApi/
- Features/
  - Properties/
    - ListProperties/
      - ListPropertiesSlice.cs (o separación equivalente Endpoint/Handler/Mapping/Validators)
- Infrastructure/
  - Slices/
  - Handlers/
  - Validation/
- Domain/
- Program.cs

Regla obligatoria:
- Crear carpeta Features en el root de RealtorApi (si no existe) y colocar allí el slice de negocio.

## Alcance funcional incluido

### 1) Catálogo persistente de estados de propiedad
- Persistir `PropertyStatus` como entidad relacional configurada mediante `IEntityTypeConfiguration<T>` en la tabla PostgreSQL `property_status`.
- Relacionar cada propiedad con un estado del catálogo mediante una clave foránea. La spec debe definir los campos y las reglas de unicidad necesarios para identificar los estados sin ambigüedad.
- Mantener los estados `Available`, `Rented` y `Maintenance`, sembrarlos idempotentemente y actualizar la lectura del seed de propiedades para resolver cada estado contra el catálogo.
- Definir una única migración coherente que convierta el esquema actual y conserve los datos y estados de las propiedades existentes. No asumir que basta con crear una tabla vacía ni eliminar o recrear propiedades.

### 2) Endpoint de listado paginado
- Método: GET
- Ruta: /api/properties
- Debe devolver lista paginada de propiedades.

### 3) Parámetros de paginación
- Query params:
  - page
  - pageSize
- Comportamiento por defecto cuando no llegan parámetros:
  - page = 1
  - pageSize = 6
- Orden obligatorio:
  - title ascendente

### 4) Contrato de respuesta
Cada item de propiedad debe incluir al menos:
- id
- title
- description
- address
- price
- status
- bedroomCount
- bathroomCount
- areaSquareMeters
- imageUrl

El campo `status` debe conservar el valor textual esperado por el contrato vigente.

Requisito obligatorio de imagen:
- Debe devolverse imageUrl correspondiente a cada propiedad.
- imageUrl debe ser URL absoluta pública servible por API.
- Formato esperado de salida: `http(s)://{host}/assets/properties/{fileName}`
- Prohibido devolver rutas físicas internas del servidor.
- Prohibido devolver imageUrl relativo en la respuesta final del endpoint.

Metadatos de paginación requeridos:
- page
- pageSize
- totalItems
- totalPages
- hasNext
- hasPrevious

### 5) Validación y errores
- Validar parámetros page y pageSize.
- Si parámetros inválidos:
  - devolver 400 con ProblemDetails o ValidationProblemDetails.
- Propagar CancellationToken en todo flujo.

### 6) Mapping y acceso a datos
- No devolver entidades EF directamente.
- Usar contratos explícitos de response.
- Para lectura, preferir proyección EF Core directa con AsNoTracking.
- Mantener endpoint delgado y lógica de caso de uso en handler del slice.
- Construir imageUrl absoluta con base en el request actual (scheme + host) durante el mapping de respuesta.

### 7) Prueba manual de endpoint
- Agregar prueba de endpoint en app/backend/src/RealtorApi/RealtorApi.http:
  - GET /api/properties
  - GET /api/properties?page=1&pageSize=6

## Criterios de aceptación
1. Dada una base de datos nueva, cuando se ejecutan la migración y el seeding, entonces existe `property_status` con `Available`, `Rented` y `Maintenance`, sin duplicados.
2. Dadas propiedades con estados existentes, cuando se aplica la migración, entonces se conservan sus datos y cada propiedad queda relacionada con el estado correspondiente.
3. Dada una propiedad de seed, cuando se persiste, entonces su estado se resuelve desde `property_status`.
4. Dada una segunda ejecución del seeding, cuando los estados y propiedades ya existen, entonces no se crean duplicados.
5. Dada una respuesta del listado, cuando se serializa una propiedad, entonces `status` conserva el valor textual esperado por el contrato vigente.
6. Dadas propiedades de seed, cuando consulto `GET /api/properties` sin parámetros, entonces devuelve la primera página con hasta 6 registros ordenados por título ascendente.
7. Dadas propiedades suficientes, cuando consulto `/api/properties?page=2&pageSize=6`, entonces devuelve la página correcta con metadatos consistentes.
8. Dados parámetros inválidos (por ejemplo, `page <= 0` o `pageSize <= 0`), cuando consulto el endpoint, entonces devuelve 400 con detalle de validación.
9. Dadas propiedades con imagen persistida, cuando consulto el listado, entonces cada propiedad incluye una `imageUrl` absoluta válida.
10. Dado el host local `http://localhost:5023`, cuando consulto el listado, entonces `imageUrl` sigue el patrón `http://localhost:5023/assets/properties/{fileName}`.

## Restricciones explícitas
- No usar controllers.
- Limitar la iniciativa al catálogo persistente `PropertyStatus` con su relación, migración y seed, y al listado paginado descrito en esta spec.
- No romper convenciones ISlice existentes.
- No introducir mapeadores automáticos no requeridos.
- Mantener el alcance aprobado durante `plan`, `tasks` e `implement`; toda implementación debe estar descrita en esta spec y trazada a una tarea específica de `tasks.md`.