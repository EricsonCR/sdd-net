# Plan de implementación: Catálogo de estados y listado paginado de propiedades

**Rama**: `feat/004-properties-list-pagination` | **Fecha**: `2026-10-15` | **Spec**: [spec.md](spec.md)

**Entrada**: Especificación de `specs/004-properties-list-pagination/spec.md`

## Resumen

Convertir el estado textual persistido de `Property` en una relación con el catálogo `property_status` sin alterar los valores ni las filas existentes; adaptar el seeding para resolver estados desde el archivo declarado en el manifiesto; y añadir `GET /api/properties` como slice auto-descubierto, con paginación estable, validación uniforme y URLs absolutas de imagen. La consulta usará proyección de lectura y mantendrá los archivos actuales, exponiéndolos también bajo `/assets/properties`.

## Contexto técnico

**Lenguaje y versión**: C# con .NET 10; SDK fijado en `global.json` a `10.0.401`.

**Dependencias principales**: ASP.NET Core Minimal APIs, EF Core 10, Npgsql 10, FluentValidation, xUnit 2 y `Microsoft.NET.Test.Sdk`.

**Almacenamiento**: PostgreSQL mediante Npgsql y migraciones EF Core; los assets permanecen en `wwwroot/images/properties`.

**Pruebas**: Pruebas unitarias xUnit en `app/backend/tests/RealtorApiTests`; validación manual PostgreSQL/HTTP documentada en `quickstart.md`. No se planifican pruebas automatizadas de integración sin aprobación explícita conforme a la constitución.

**Plataforma objetivo**: API ASP.NET Core 10 con PostgreSQL y assets estáticos servidos por la API.

**Tipo de proyecto**: Backend HTTP Minimal API dentro de la solución Realtor existente.

**Objetivos de rendimiento**: La spec no establece SLO de latencia ni volumen. Limitar cada página a 50 elementos, contar los resultados y recuperar solo la página solicitada con orden aplicado en base de datos.

**Restricciones**: Una sola migración para el cambio funcional; no alterar la iniciativa 003; conservar `Properties.Status` como texto y usarlo como FK hacia `property_status.Value`; no controllers, registros manuales, mapper automático ni autenticación para este GET; validar el host y confiar en forwarded headers solo desde proxies configurados; respetar `ISlice`, el registro automático y los patrones de persistencia vigentes.

**Alcance**: Catálogo inicial de tres estados, relación obligatoria, seeding idempotente y listado público paginado. El seed actual contiene 10 propiedades; no se define escala de producción ni se añaden filtros.

## Comprobación constitucional

**Gate previo a investigación**

- ✅ Se mantiene la solución compartida y la ubicación canónica `specs/004-properties-list-pagination/`.
- ✅ Se usa .NET 10, que coincide con la constitución, `global.json` y los proyectos `net10.0`. La instrucción de backend que indica .NET 11 es inconsistente y no prevalece sobre la constitución; no se modifica en esta iniciativa.
- ✅ Se conserva Minimal API, Vertical Slice, `ISlice` auto-descubierto y ausencia de controllers o registros individuales en `Program.cs`.
- ✅ EF Core/Npgsql/PostgreSQL, configuraciones `IEntityTypeConfiguration<T>`, migración versionada y `UseSeeding`/`UseAsyncSeeding` permanecen dentro de las reglas vigentes.
- ✅ La migración preserva filas y valores; el seeding no usa `HasData` ni reemplaza estados desconocidos.
- ✅ Se permiten únicamente las salidas auxiliares de planificación autorizadas; `tasks.md` pertenece a la fase posterior.
- ✅ La constitución limita las pruebas automatizadas a unitarias salvo aprobación explícita de otros tipos; PostgreSQL y HTTP quedan como escenarios manuales del quickstart.

**Reevaluación posterior al diseño**

- ✅ El catálogo conserva la clave textual existente y aplica FK sin reconstruir `Properties`.
- ✅ La validación de query evita que el binding numérico previo al filtro eluda el contrato `400` con `ValidationProblemDetails`.
- ✅ La URL pública se calcula en mapping desde la petición y un alias estático sirve los archivos existentes sin moverlos.
- ✅ Los hosts se validan con una lista explícita y los forwarded headers requieren proxies confiables configurados.
- ✅ Una imagen persistida inválida falla con ProblemDetails sin devolver una página parcial.
- ✅ Los contratos de respuesta no exponen entidades EF y el handler recibe/proyecta solo los datos del slice.
- ✅ No hay excepciones constitucionales que requieran una sección de complejidad.

## Estructura del repositorio

### Documentación de esta iniciativa

```text
specs/004-properties-list-pagination/
├── spec.md
├── plan.md
├── research.md
├── data-model.md
├── quickstart.md
└── contracts/
    └── properties-list.md
```

`tasks.md` se generará exclusivamente en la fase `speckit.tasks`.

### Código fuente

```text
app/backend/src/RealtorApi/
├── Domain/
│   └── Properties/
│       ├── Property.cs
│       └── PropertyStatus.cs
├── Features/
│   └── Properties/
│       └── ListProperties/
│           ├── ListPropertiesEndpoint.cs
│           ├── ListPropertiesHandler.cs
│           ├── ListPropertiesModels.cs
│           ├── ListPropertiesValidator.cs
│           └── ListPropertiesMapping.cs
├── Infrastructure/
│   ├── Endpoints/
│   ├── Handlers/
│   ├── Validation/
│   ├── Persistence/
│   │   └── Configurations/
│   └── Migrations/
├── Program.cs
└── RealtorApi.http

app/backend/tests/RealtorApiTests/
├── Features/Properties/ListProperties/
└── Infrastructure/Persistence/
```

**Decisión de estructura**: La nueva lógica de negocio reside en `Features/Properties/ListProperties`; se reutilizan los scanners y el filtro existentes en `Infrastructure/Endpoints`, `Infrastructure/Handlers` e `Infrastructure/Validation`. Las configuraciones siguen en `Infrastructure/Persistence/Configurations`. Las migraciones continúan en `Infrastructure/Migrations`, donde están las migraciones actuales. Los nombres de archivo del slice son orientativos y solo se crearán los necesarios; `Program.cs` solo recibe el registro de ruta estática requerido por el contrato de assets, sin registrar el slice manualmente.

## Artefactos de diseño

- Investigación y decisiones: [research.md](research.md).
- Entidades y relación: [data-model.md](data-model.md).
- Contrato HTTP: [contracts/properties-list.md](contracts/properties-list.md).
- Procedimiento de validación manual: [quickstart.md](quickstart.md).
