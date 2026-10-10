# Modelo de datos de la fundación

## Visión general

Esta iniciativa no crea entidades de dominio ni modelos persistentes. Su objetivo es preparar la base del sistema y dejar la estructura para que las siguientes especificaciones agreguen negocio y persistencia de manera controlada.

## Entidades del sistema

### 1. `HealthResponse`

| Campo | Tipo | Requerido | Descripción |
|---|---|---:|---|
| `status` | `string` | Sí | Estado del servicio, normalmente `Healthy`. |
| `service` | `string` | Sí | Nombre del servicio que responde. |
| `timestamp` | `DateTimeOffset` | Sí | Marca temporal UTC de la respuesta. |

**Uso**: respuesta de `GET /health` en el backend.

## Relaciones y persistencia

- No existen relaciones de base de datos en esta especificación.
- No hay migraciones ni modelos EF Core.
- Los objetos de respuesta son temporales y solo existen en memoria durante la ejecución.

## Reglas de validación

- La ruta `GET /health` no recibe parámetros ni cuerpo.
- La respuesta debe retornar un estado HTTP exitoso y un JSON válido.
- El contenido del JSON debe ser simple y estable para que pueda usarse en comprobaciones automáticas.

## Estado y transiciones

No aplica. La iniciativa no introduce flujo de negocio ni estados de dominio.

## Alcance fuera de modelo

- No se crean entidades de negocio, clases de dominio, repositorios ni DbContext.
- No se implementa autenticación ni autorización.
- No se define persistencia de usuarios, propiedades, contratos ni operaciones de Realtor.
