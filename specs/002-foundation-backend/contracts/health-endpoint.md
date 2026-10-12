# Contrato de salud preservado por la fundación backend

## Fuente normativa

El contrato HTTP completo de `GET /health` se define en `specs/001-solution-foundation/contracts/health-endpoint.md`. Esta iniciativa no lo modifica; cambia únicamente la implementación interna para que la ruta sea una slice descubierta y mapeada centralmente.

## Comportamiento conservado

- Método y ruta: `GET /health`.
- Respuesta satisfactoria: `200 OK` con JSON.
- El payload incluye `status` igual a `Healthy`, `service` igual a `RealtorApi` y `timestamp`.
- La respuesta no depende de base de datos, servicios externos ni lógica de negocio.

## Verificación

Una solicitud HTTP al host de tests debe confirmar el status y los campos definidos en la fuente normativa, después del descubrimiento de `ISlice` y el mapeo centralizado.