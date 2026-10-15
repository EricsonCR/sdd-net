# Contrato HTTP: Listado de propiedades

**Iniciativa**: `004-properties-list-pagination`  
**Spec**: [../spec.md](../spec.md)  
**Plan**: [../plan.md](../plan.md)

## Listar propiedades

`GET /api/properties`

Acceso anónimo. No requiere cuerpo, autenticación ni encabezado de autorización.

### Query parameters

| Nombre | Tipo | Predeterminado | Restricciones |
|---|---|---:|---|
| `page` | Entero positivo | `1` | Si se envía, debe ser entero positivo; en otro caso responde HTTP `400`. Si se omite, usa `1`. |
| `pageSize` | Entero positivo | `6` | Si se envía, debe ser entero entre `1` y `50`; en otro caso responde HTTP `400`. Si se omite, usa `6`. |

Los parámetros inválidos responden HTTP `400` con `ValidationProblemDetails`. Los elementos se ordenan por `title` ascendente y, como desempate, `id` ascendente. Una página posterior a la última responde `200` con `items` vacío. El host usado en las URLs debe estar permitido por `AllowedHosts`; no se confía en forwarded headers salvo que el proxy esté configurado como confiable.

### Respuesta `200 OK`

```json
{
  "items": [
    {
      "id": "00000000-0000-0000-0000-000000000001",
      "title": "Casa de ejemplo",
      "description": "Descripción de ejemplo",
      "address": "Dirección de ejemplo",
      "price": 4200.00,
      "status": "Available",
      "bedroomCount": 3,
      "bathroomCount": 2,
      "areaSquareMeters": 185.50,
      "imageUrl": "http://localhost:5023/assets/properties/1.png"
    }
  ],
  "page": 1,
  "pageSize": 6,
  "totalItems": 10,
  "totalPages": 2,
  "hasNext": true,
  "hasPrevious": false
}
```

`imageUrl` siempre es absoluta para elementos devueltos y corresponde al esquema, host y puerto de la solicitud. La ruta pública es `/assets/properties/{fileName}`; no se exponen rutas de filesystem ni valores relativos.

### Respuesta `400 Bad Request`

Se devuelve `application/problem+json` con `ValidationProblemDetails`, `status: 400` y un objeto `errors` que identifica `page` o `pageSize` y explica la restricción incumplida.

Una solicitud cuyo host no está permitido responde `application/problem+json` con `ProblemDetails`, `status: 400`; ese host nunca se incluye en `imageUrl`.

### Respuesta `500 Internal Server Error`

Si una propiedad incluida en la página no contiene un nombre de archivo de imagen utilizable, se devuelve `application/problem+json` con `ProblemDetails`, `status: 500`, sin entregar una página parcial.

### Recursos de imagen

Cada `imageUrl` devuelto es una URL GET pública que sirve el archivo asociado en el tipo de contenido correspondiente. No introduce una operación de negocio ni un controlador; utiliza el servicio estático de archivos existente con el alias de ruta requerido.