# Contrato de manifest central de seeding

## Propósito

Definir la estructura del archivo central que resuelve los recursos de seed para estados, propiedades e imágenes.

## Archivo

`support/seed-data/seed-manifest.json`

## Esquema esperado

```json
{
  "version": 1,
  "sourceRoot": "support/seed-data",
  "propertyStatusesFile": "properties-statuses.json",
  "propertiesFile": "properties.json",
  "imageSourceFolder": "images/properties",
  "imageDestinationFolder": "wwwroot/images/properties",
  "publicBasePath": "/images/properties"
}
```

## Reglas

- El manifest es la única fuente de resolución de los archivos de seed.
- Las rutas deben ser relativas al directorio de soporte del proyecto.
- El destino de las imágenes debe coincidir con la ruta pública final servida por la API.
- La ruta final de `ImageUrl` no debe incluir rutas físicas de `support`.
