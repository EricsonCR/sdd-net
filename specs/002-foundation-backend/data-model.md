# Modelo de datos de la fundación interna del backend

## Entidades de dominio y persistencia

Esta iniciativa no introduce entidades de dominio, relaciones persistentes ni almacenamiento. No crea `DbContext`, configuraciones EF Core, migraciones, seeders o conexiones a base de datos.

## Contratos técnicos en memoria

### `Result<T>`

Representa el éxito o el fallo esperado de una operación futura. El resultado exitoso transporta un valor de tipo `T`; el fallido transporta un error. No define casos de uso ni reglas de negocio en esta iniciativa.

### Error esperado

| Campo | Tipo conceptual | Requerido | Uso |
|---|---|---:|---|
| `code` | Cadena | Sí | Código estable, expuesto como extensión `code` en `ProblemDetails`. |
| `title` | Cadena | Sí | Título del problema esperado. |
| `detail` | Cadena | Sí | Descripción pública del problema. |
| `status` | Entero HTTP | Sí | Código HTTP que se asigna a `ProblemDetails`. |

El error no contiene información de excepción, stack trace ni datos internos.

### Resultado de validación

Los errores de validación de entrada no son errores del modelo `Result`: se devuelven como `ValidationProblemDetails` con HTTP 400 y agrupados por nombre de propiedad.

## Relaciones y persistencia

No aplica. `ISlice`, `IHandler`, `Result<T>` y el error son contratos técnicos; existen solo en memoria y no son entidades de dominio.

## Estado y transiciones

No se definen estados de producto ni transiciones de negocio.