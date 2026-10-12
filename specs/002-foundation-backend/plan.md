# Plan de implementación: Fundación interna del backend Realtor

**Rama**: `002-foundation-backend` | **Fecha**: `2026-10-12` | **Spec**: `specs/002-foundation-backend/spec.md`

**Entrada**: Especificación de `specs/002-foundation-backend/spec.md`

## Resumen

Establecer infraestructura transversal para el backend RealtorApi existente: descubrimiento de slices Minimal API, registro por assembly de handlers y validators, validación convencional, resultados de errores esperados y conversión a ProblemDetails. La sonda `/health` será la única implementación de `ISlice`; no se crearán casos de uso ni elementos de dominio.

## Contexto técnico

**Lenguaje y versión**: C# con .NET 10 (`net10.0`), usando el SDK `10.0.401` de `global.json` en la raíz del repositorio.

**Dependencias principales**: ASP.NET Core Minimal APIs y DI del framework; `FluentValidation.DependencyInjectionExtensions` para escaneo de validators. `Result`, el error esperado y los mapeos se implementan como contratos internos, sin librería externa de resultados.

**Almacenamiento**: No aplica. No se agrega EF Core, Npgsql, PostgreSQL ni conexión a datos en esta iniciativa.

**Pruebas**: xUnit en `app/backend/tests/RealtorApiTests/`. Las comprobaciones de scanners, validación y Result son unitarias. La única prueba HTTP en proceso usa `WebApplicationFactory` para `/health`; su ejecución como excepción constitucional queda condicionada a que la spec esté Aprobada y exista una tarea explícita en `tasks.md`.

**Plataforma objetivo**: ASP.NET Core sobre .NET 10, multiplataforma.

**Tipo de proyecto**: Backend Minimal API existente dentro de la solución compartida.

**Objetivos de rendimiento**: No se establecen objetivos cuantitativos; los scanners se ejecutan al configurar la aplicación, no por solicitud.

**Restricciones**: No recrear solución ni proyecto; no modificar `global.json`; no usar controllers; no agregar negocio, persistencia o dependencias externas. Las instrucciones locales de backend mencionan .NET 11, pero la constitución y el SDK fijado establecen .NET 10 y prevalecen para esta iniciativa.

**Escala y alcance**: Contratos y capacidades transversales únicamente; una sonda de infraestructura; ninguna feature de producto.

## Verificación de la constitución

*Puerta de control previa a investigación y revisión posterior al diseño.*

- ✅ La iniciativa extiende la solución existente y no crea proyectos paralelos.
- ✅ Usa Minimal APIs y organiza las capacidades compartidas bajo `Infrastructure/`; no introduce controllers ni slices de negocio.
- ✅ Respeta .NET 10 y `global.json`; la divergencia de .NET 11 en instrucciones backend queda resuelta por precedencia constitucional.
- ✅ No agrega persistencia, entidades, migraciones, seeders ni conexiones de base de datos.
- ✅ Los artefactos auxiliares de esta fase están permitidos por la enmienda constitucional 1.1.0 y se referencian aquí.
- ✅ La prueba `WebApplicationFactory` se limita a `/health`; la aprobación quedó registrada en la spec y T017 identifica expresamente esa prueba antes de iniciar la implementación.

## Decisiones de diseño

- `ISlice` será stateless y se registrará como singleton. `MapSliceEndpoints` resolverá las implementaciones durante el mapeo de rutas y delegará en `AddEndpoint`.
- `RegisterSlices` inspeccionará únicamente el assembly recibido, filtrará clases públicas, concretas y no abstractas que implementen `ISlice`, y evitará descriptores duplicados.
- `IHandler` será un marker. El scanner registrará cada implementación concreta como servicio scoped; no se agrega dispatcher ni handler concreto. Las slices no capturarán handlers scoped en constructores.
- FluentValidation registrará validators del assembly indicado con lifetime scoped. La factory inspeccionará los parámetros del handler y validará aquellos que tengan un `IValidator<T>` registrado; parámetros sin validator pasan sin alteración. Usará `ValidateAsync` y propagará `CancellationToken`.
- Un error de validación genera `400` con `ValidationProblemDetails`. Un error esperado de `Result<T>` se convierte centralmente a `ProblemDetails`, copiando `title`, `detail`, `status` y la extensión `code`.
- Los servicios usarán `ILogger<T>` del framework cuando corresponda; no se crean proveedores, logs de negocio ni eventos artificiales.
- `/health` se moverá a `Infrastructure/Endpoints` como slice stateless, manteniendo el contrato existente de la spec 001.
- `Program.cs` solo configurará servicios y middleware, registrará las capacidades transversales, construirá la aplicación y llamará a `MapSliceEndpoints`.

## Estructura del proyecto

### Artefactos de la iniciativa

```text
specs/002-foundation-backend/
├── spec.md
├── plan.md
├── tasks.md                 # Se genera en la fase speckit.tasks
├── research.md
├── data-model.md
├── quickstart.md
└── contracts/
    └── health-endpoint.md
```

### Backend y pruebas

```text
app/backend/src/RealtorApi/
├── Program.cs
├── Infrastructure/
│   ├── Endpoints/
│   │   ├── ISlice.cs
│   │   ├── SliceRegistrationExtensions.cs
│   │   ├── SliceEndpointExtensions.cs
│   │   └── HealthEndpoint.cs
│   ├── Handlers/
│   │   ├── IHandler.cs
│   │   └── HandlerRegistrationExtensions.cs
│   ├── Results/
│   │   ├── Result.cs
│   │   ├── Error.cs
│   │   └── ProblemDetailsMapping.cs
│   └── Validation/
│       ├── ValidationServiceCollectionExtensions.cs
│       └── ValidationFilterFactory.cs
└── appsettings*.json

app/backend/tests/RealtorApiTests/
├── HealthEndpointTests.cs
└── Infrastructure/
    ├── Endpoints/
    ├── Validation/
    └── Results/
```

No se crea una carpeta de feature vacía. Los tipos de prueba para `ISlice` y validators viven solo en el proyecto de tests; el proyecto de producción no contiene implementaciones de negocio de `IHandler` ni validators de producto.

## Fase 0: Investigación

La investigación y sus decisiones están consolidadas en `research.md`. No quedan incógnitas tecnológicas que requieran cambiar el stack fijado. Las únicas condiciones pendientes son de gobernanza para habilitar la prueba HTTP de salud, no decisiones de diseño.

## Fase 1: Diseño y contratos

- `data-model.md` documenta la ausencia de entidades persistentes y los contratos técnicos `Result<T>` y error.
- `contracts/health-endpoint.md` referencia como fuente normativa el contrato de salud de `specs/001-solution-foundation/contracts/health-endpoint.md`; no cambia el contrato HTTP externo.
- `quickstart.md` describe compilación, pruebas permitidas y verificación HTTP de `/health`.

## Revisión constitucional posterior al diseño

- ✅ Los contratos no agregan entidades de dominio y la solución existente se conserva.
- ✅ Las dependencias propuestas se limitan a validación y capacidades del framework necesarias para los requisitos.
- ✅ El endpoint público mantiene su contrato previo; los contratos nuevos de negocio y la persistencia siguen fuera de alcance.
- ✅ La excepción de prueba de host está autorizada únicamente para `/health`; la aprobación y transición a En implementación constan en la spec y T017 proporciona trazabilidad explícita.

## Seguimiento de excepciones

| Excepción | Justificación | Límite y condición |
|---|---|---|
| Una prueba `WebApplicationFactory` para `GET /health` | Verifica que el host descubre y mapea la slice real y mantiene el contrato HTTP requerido. | Solo `/health`, sin servicios externos ni base de datos. La spec registra su aprobación previa y T017 la traza explícitamente. |
