# Modelo de datos: propiedades y estados

## Entidad `PropertyStatus`

`PropertyStatus` es un enum que representa el estado dinámico de una propiedad.

```csharp
public enum PropertyStatus
{
    Available,
    Rented,
    Maintenance
}
```

**Persistencia**: EF Core debe convertirlo a `string` usando configuración específica.

## Entidad `Property`

```csharp
public class Property
{
    public Guid Id { get; set; }
    public string Title { get; set; } = default!;
    public string Description { get; set; } = default!;
    public string Address { get; set; } = default!;
    public decimal Price { get; set; }
    public PropertyStatus Status { get; set; }
    public int BedroomCount { get; set; }
    public int BathroomCount { get; set; }
    public decimal AreaSquareMeters { get; set; }
    public string ImageUrl { get; set; } = default!;
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset? UpdatedAt { get; set; }
}
```

## Restricciones sugeridas

- `Title`: requerido, longitud controlada.
- `Description`: requerido, con longitud suficiente para contenido descriptivo.
- `Address`: requerido, longitud razonable.
- `Price`: decimal con precisión y escala apropiadas.
- `Status`: almacenado como `string` y validado por valores del enum.
- `ImageUrl`: requerido, de tipo texto corto a medio.
- `CreatedAt`: default a la hora actual.
- `UpdatedAt`: nullable para indicar última edición.

## Reglas de seed

La semántica de seed debe usar un conjunto base de propiedades asociado a los estados de `PropertyStatus` y a imágenes provistas por `support/seed-data`. Las propiedades deben ser insertadas con `Id` determinista si se necesita comparación o idempotencia directa.

## Contracto de serialización

- `Status` serializa como texto en base de datos, por ejemplo `Available`.
- `ImageUrl` será una ruta pública relativa o absoluta al host, por ejemplo `/images/properties/1.png`.
- `CreatedAt` y `UpdatedAt` siguen el patrón UTC con `DateTimeOffset`.
