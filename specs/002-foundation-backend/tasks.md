# Tareas: Fundación interna del backend Realtor

**Entrada**: Documentos de diseño de `specs/002-foundation-backend/`

**Prerrequisitos**: `spec.md`, `plan.md`, `research.md`, `data-model.md`, `contracts/`

**Pruebas**: Requeridas explícitamente por la spec. Las pruebas son unitarias salvo una única prueba HTTP con `WebApplicationFactory` para `/health`, condicionada a que la spec esté Aprobada.

## Fase 1: Preparación

**Propósito**: Confirmar el punto de partida sin recrear la solución ni los proyectos generados en la spec 001.

No requiere tareas de inicialización: `app/Realtor.sln`, el backend y el proyecto de tests ya existen. Las dependencias nuevas se incorporan junto con la historia que las necesita.

## Fase 2: Fundamentos

**Propósito**: Preparar requisitos compartidos que bloqueen todas las historias.

No requiere tareas adicionales: el proyecto, la solución y el test project existentes proporcionan la base. La infraestructura transversal se entrega mediante las historias siguientes, para mantener pruebas y cambios vinculados a sus criterios de aceptación.

## Fase 3: Historia 1 - Descubrir y mapear endpoints (Prioridad: P1)

**Objetivo**: Definir `ISlice`, descubrir implementaciones concretas por assembly y mapearlas centralmente, sin modificar `Program.cs` para registrar endpoints individuales.

**Prueba independiente**: Una implementación de prueba de `ISlice` en el assembly de tests queda registrada bajo `ISlice` exactamente una vez.

### Contrato previo y pruebas de la historia 1

- [X] T001 [US1] Definir el contrato `ISlice` con `AddEndpoint(IEndpointRouteBuilder app)` en `app/backend/src/RealtorApi/Infrastructure/Endpoints/ISlice.cs`.
- [X] T002 [US1] Crear una prueba con una slice pública de prueba declarada en el assembly de tests que verifique el registro bajo `ISlice` sin duplicados en `app/backend/tests/RealtorApiTests/Infrastructure/Endpoints/SliceRegistrationTests.cs`.

### Implementación de la historia 1

- [X] T003 [P] [US1] Implementar `RegisterSlices(IServiceCollection, Assembly)` para descubrir por reflection tipos públicos, concretos y no abstractos que implementen `ISlice`, registrarlos como singleton sin duplicados en `app/backend/src/RealtorApi/Infrastructure/Endpoints/SliceRegistrationExtensions.cs`.
- [X] T004 [P] [US1] Implementar `MapSliceEndpoints` para crear el grupo central, resolver `IEnumerable<ISlice>` y llamar `AddEndpoint` sin registros manuales en `app/backend/src/RealtorApi/Infrastructure/Endpoints/SliceEndpointExtensions.cs`.

**Checkpoint**: La prueba de registro pasa y las rutas se pueden mapear desde las slices descubiertas.

## Fase 4: Historia 2 - Aplicar validación automática a Minimal APIs (Prioridad: P1)

**Objetivo**: Escanear validators del assembly indicado y validar automáticamente los parámetros de endpoint para los que exista `IValidator<T>`, con pass-through cuando no exista.

**Prueba independiente**: Las pruebas unitarias verifican validator presente, ausencia de validator y respuesta `400` con `ValidationProblemDetails` para una validación fallida.

### Dependencia y pruebas de la historia 2

- [X] T005 [US2] Agregar la dependencia `FluentValidation.DependencyInjectionExtensions` compatible con .NET 10 al proyecto `app/backend/src/RealtorApi/RealtorApi.csproj`.
- [X] T006 [US2] Crear pruebas unitarias de `ValidationFilterFactory` para ejecución del validator, pass-through sin validator y fallo HTTP 400 con `ValidationProblemDetails` en `app/backend/tests/RealtorApiTests/Infrastructure/Validation/ValidationFilterFactoryTests.cs`.

### Implementación de la historia 2

- [X] T007 [P] [US2] Implementar el registro scoped de validators mediante assembly scanning, sin registros individuales, en `app/backend/src/RealtorApi/Infrastructure/Validation/ValidationServiceCollectionExtensions.cs`.
- [X] T008 [P] [US2] Implementar `ValidationFilterFactory` para detectar parámetros con `IValidator<T>`, usar `ValidateAsync`, propagar `CancellationToken`, continuar si no hay validator y devolver `400` con `ValidationProblemDetails` si falla en `app/backend/src/RealtorApi/Infrastructure/Validation/ValidationFilterFactory.cs`.
- [X] T009 [US2] Aplicar `ValidationFilterFactory` al grupo central de endpoints sin agregar filtros individuales en `app/backend/src/RealtorApi/Infrastructure/Endpoints/SliceEndpointExtensions.cs`.

**Checkpoint**: La validación se descubre automáticamente y el caso sin validator no interrumpe la solicitud.

## Fase 5: Historia 3 - Estandarizar errores esperados y handlers (Prioridad: P2)

**Objetivo**: Proporcionar el marker `IHandler`, su registro automático y los contratos `Result<T>`/error con conversión central a `ProblemDetails`, sin crear handlers de negocio.

**Prueba independiente**: Las pruebas verifican el escaneo de un handler de prueba declarado en el proyecto de tests y que el mapeo del error conserve status y payload.

### Contratos previos y pruebas de la historia 3

- [X] T010 [US3] Definir el marker `IHandler` en `app/backend/src/RealtorApi/Infrastructure/Handlers/IHandler.cs`.
- [X] T011 [P] [US3] Definir el error esperado con `code`, `title`, `detail` y `status` en `app/backend/src/RealtorApi/Infrastructure/Results/Error.cs`.
- [X] T012 [US3] Definir `Result<T>` para representar éxito con valor y fallo con error esperado en `app/backend/src/RealtorApi/Infrastructure/Results/Result.cs`.
- [X] T013 [P] [US3] Crear una prueba unitaria que verifique el status, título, detalle y extensión `code` al convertir un error a `ProblemDetails` en `app/backend/tests/RealtorApiTests/Infrastructure/Results/ProblemDetailsMappingTests.cs`.
- [X] T014 [US3] Crear una prueba unitaria con un handler de prueba declarado en el assembly de tests que verifique el auto-registro de implementaciones concretas de `IHandler` en `app/backend/tests/RealtorApiTests/Infrastructure/Handlers/HandlerRegistrationTests.cs`.

### Implementación de la historia 3

- [X] T015 [P] [US3] Implementar el escaneo del assembly indicado y el registro scoped de handlers concretos que implementen `IHandler`, sin lista manual ni dispatcher, en `app/backend/src/RealtorApi/Infrastructure/Handlers/HandlerRegistrationExtensions.cs`.
- [X] T016 [US3] Implementar la conversión central de errores `Result<T>` a `ProblemDetails`, copiando `title`, `detail`, `status` y la extensión `code`, en `app/backend/src/RealtorApi/Infrastructure/Results/ProblemDetailsMapping.cs`.

**Checkpoint**: Los errores esperados tienen un contrato común y los handlers se registran por assembly sin implementar casos de uso.

## Fase 6: Historia 4 - Comprobar la disponibilidad mediante `/health` (Prioridad: P2)

**Objetivo**: Migrar la única sonda de infraestructura a `ISlice`, conservar su contrato y confirmar que se descubre y mapea desde el host.

**Prueba independiente**: Una solicitud HTTP al host de tests devuelve `200` y JSON con `status`, `service` y `timestamp` después del mapeo central.

**Condición obligatoria**: No iniciar ni marcar como completada la prueba HTTP hasta que `specs/002-foundation-backend/spec.md` esté en estado Aprobada. Esta es la única prueba de host autorizada por esta spec.

### Prueba de la historia 4

- [X] T017 [US4] Después de aprobar `specs/002-foundation-backend/spec.md`, actualizar la prueba existente para verificar con `WebApplicationFactory` exclusivamente `GET /health`, HTTP 200 y los campos `status`, `service` y `timestamp` en `app/backend/tests/RealtorApiTests/HealthEndpointTests.cs`.

### Implementación de la historia 4

- [X] T018 [US4] Migrar `GET /health` a una implementación stateless de `ISlice` que conserve el payload aprobado en `app/backend/src/RealtorApi/Infrastructure/Endpoints/HealthEndpoint.cs`.
- [X] T019 [US4] Configurar el assembly de producción para registrar slices, handlers y validators, y mapear rutas con `MapSliceEndpoints`, eliminando el mapping manual de salud en `app/backend/src/RealtorApi/Program.cs`.

**Checkpoint**: `/health` se descubre y mapea como slice, mantiene el contrato anterior y no depende de negocio ni servicios externos.

## Fase 7: Pulido y validación transversal

**Propósito**: Validar la compilación, las pruebas autorizadas y las restricciones de alcance.

- [X] T020 [P] Revisar que el proyecto de producción no contenga entidades, persistencia, endpoints ni casos de uso de negocio en `app/backend/src/RealtorApi/`.
- [X] T021 Ejecutar la compilación y las pruebas permitidas descritas en `specs/002-foundation-backend/quickstart.md` después de registrar la aprobación de la spec; T017 limita la prueba HTTP a `/health`.

## Dependencias y orden de ejecución

### Dependencias de fases

- **Preparación (Fase 1)**: No recrea nada; la solución y los proyectos existen por la spec 001.
- **Fundamentos (Fase 2)**: No agrega tareas independientes; las historias entregan la infraestructura por incrementos.
- **Historia 1 (Fase 3)**: No depende de otras historias.
- **Historia 2 (Fase 4)**: Depende de la Historia 1 porque conecta la factory al grupo central de endpoints.
- **Historia 3 (Fase 5)**: Sus contratos y scanners son independientes de las Historias 1 y 2; puede ejecutarse en paralelo una vez que la base existente esté disponible.
- **Historia 4 (Fase 6)**: Depende de las Historias 1 y 2 para descubrir, mapear y aplicar el filtro central al endpoint.
- **Pulido (Fase 7)**: Depende de las historias que se validen; T021 depende de la aprobación de la spec para ejecutar la única prueba HTTP.

### Dependencias entre historias

- **US1 (P1)**: Independiente; entrega el contrato y mapeo central de slices.
- **US2 (P1)**: Depende de US1 para incorporar el filtro a `MapSliceEndpoints`.
- **US3 (P2)**: Independiente de US1 y US2; solo comparte el proyecto backend y los contratos de infraestructura.
- **US4 (P2)**: Depende de US1 y US2 para probar `/health` mediante el pipeline central.

### Oportunidades de paralelismo

- **US1**: T003 y T004 modifican archivos diferentes y pueden realizarse en paralelo después de definir `ISlice` y preparar T002.
- **US2**: T007 y T008 pueden realizarse en paralelo una vez agregada la dependencia de T005 y creada la prueba T006.
- **US3**: T015 y T016 pueden realizarse en paralelo después de T013 y T014; T016 usa los contratos definidos por T011 y T012.
- **Entre historias**: US3 puede avanzar en paralelo con US2; US4 debe esperar a US1 y US2.

### Ejemplo de ejecución paralela

```text
US1, después de T001 y T002:
- T003 RegisterSlices en SliceRegistrationExtensions.cs
- T004 MapSliceEndpoints en SliceEndpointExtensions.cs

US2, después de T005 y T006:
- T007 escaneo de validators en ValidationServiceCollectionExtensions.cs
- T008 ValidationFilterFactory en ValidationFilterFactory.cs

US3, después de T013 y T014:
- T015 escaneo de handlers en HandlerRegistrationExtensions.cs
- T016 mapeo de Result a ProblemDetails en ProblemDetailsMapping.cs
```

## Estrategia de implementación

### MVP

El MVP recomendado es la **Historia 1**: un contrato `ISlice` y el descubrimiento/mapeo central, probado con un slice declarado solo en el assembly de tests. Esto establece el punto de extensión sin agregar endpoints de negocio. La Historia 2 completa el baseline P1 al conectar la validación global.

### Entrega incremental

1. Completar US1 y verificar el registro único y el mapeo central.
2. Completar US2 y verificar detección, pass-through y `ValidationProblemDetails`.
3. Completar US3 y verificar auto-registro de handlers y conversión a `ProblemDetails`.
4. Aprobar la spec antes de la prueba HTTP; completar US4 y validar `/health` con el pipeline central.
5. Ejecutar el quickstart y revisar las restricciones de alcance.

## Resumen de tareas

| Grupo | Cantidad |
|---|---:|
| Preparación | 0 |
| Fundamentos | 0 |
| US1 | 4 |
| US2 | 5 |
| US3 | 7 |
| US4 | 3 |
| Pulido | 2 |
| **Total** | **21** |

Todos los trabajos de implementación se agrupan por historia, usan IDs secuenciales y especifican la ruta exacta del archivo que modifican o crean.