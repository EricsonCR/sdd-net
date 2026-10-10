# Contrato del endpoint de salud

## Endpoint

`GET /health`

## Descripción

Devuelve un estado saludable del servicio para confirmar que la API se ha iniciado sin depender de funcionalidades de negocio.

## Requisitos HTTP

- Método: `GET`
- Ruta: `/health`
- Formato: `application/json`
- Código de éxito: `200 OK`

## Ejemplo de respuesta

```json
{
  "status": "Healthy",
  "service": "RealtorApi",
  "timestamp": "2026-10-10T00:00:00.0000000Z"
}
```

## Reglas del contrato

- La respuesta debe ser determinista y no depender de infraestructura externa.
- La respuesta no debe incluir estructuras de dominio ni lógica del producto.
- La ruta no recibe parámetros ni body.
- Cualquier otro método HTTP sobre la misma ruta debe rechazarse con `405 Method Not Allowed` si la aplicación lo gestiona por defecto.

## Criterio de aceptación

La implementación cumple con el contrato si la llamada a `GET /health` devuelve HTTP 200 y un payload JSON con un estado saludable.
