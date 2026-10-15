# Tareas: Catálogo de estados y listado paginado de propiedades

**Entrada**: Documentos de diseño de `specs/004-properties-list-pagination/`

**Prerequisitos**: `spec.md`, `plan.md`, `research.md`, `data-model.md`, `contracts/properties-list.md`, `quickstart.md`.

**Pruebas**: Se incluyen tareas de pruebas unitarias para actualizar el contrato persistente existente y verificar reglas del seeder y la validación. No se agregan pruebas automatizadas de integración; la migración real de PostgreSQL y el flujo HTTP se verifican manualmente según `quickstart.md`.

**Organización**: Las tareas se agrupan por historia de usuario para mantener trazabilidad e incrementos verificables.

## Fase 1: Preparación

**Propósito**: Confirmar las convenciones existentes que condicionan los cambios antes de modificar el modelo.

- [X] T001 [P] Verificar el mapeo textual y la migración base de `Property.Status` en `app/backend/src/RealtorApi/Infrastructure/Persistence/Configurations/PropertyConfiguration.cs` y `app/backend/src/RealtorApi/Infrastructure/Migrations/20261012105814_AddPropertyPersistence.cs`.
- [X] T002 [P] Confirmar los valores y rutas autorizados para el seed en `support/seed-data/seed-manifest.json`, `support/seed-data/properties-statuses.json` y `support/seed-data/properties.json`.

## Fase 2: Fundamentos compartidos

**Propósito**: Verificar prerrequisitos compartidos para las historias.

La infraestructura común de `ISlice`, registro de handlers, validación y persistencia ya existe en `app/backend/src/RealtorApi/Infrastructure/`; no se requiere una tarea fundacional ni un scanner nuevo. Completar T001-T002 antes de iniciar las historias.

## Fase 3: Historia de usuario 1 - Mantener estados consistentes (Prioridad: P1)

**Objetivo**: Persistir el catálogo relacional con una FK obligatoria que conserva el texto y las propiedades existentes.

**Prueba independiente**: En una base nueva aparecen los tres estados canónicos; en una copia de una base 003 todas las propiedades conservan sus campos y `Status` resuelve a una fila del catálogo. Un estado histórico desconocido aborta la migración sin dejar cambios parciales.

### Implementación de la Historia 1

- [X] T003 [P] [US1] Sustituir el enum por la entidad `PropertyStatus` con `Value` textual y `Description` requerida en `app/backend/src/RealtorApi/Domain/Properties/PropertyStatus.cs`.
- [X] T004 [P] [US1] Actualizar `Property` para relacionarse obligatoriamente con `PropertyStatus` y conservar el campo de estado textual en `app/backend/src/RealtorApi/Domain/Properties/Property.cs`.
- [X] T005 [P] [US1] Crear `PropertyStatusConfiguration` con tabla `property_status`, clave primaria y restricciones de campos en `app/backend/src/RealtorApi/Infrastructure/Persistence/Configurations/PropertyStatusConfiguration.cs`.
- [X] T006 [P] [US1] Actualizar `PropertyConfiguration` para mapear la FK requerida al valor de estado y mantener la columna PostgreSQL existente `Properties.Status` en `app/backend/src/RealtorApi/Infrastructure/Persistence/Configurations/PropertyConfiguration.cs`.
- [X] T007 [P] [US1] Registrar `DbSet<PropertyStatus>` en `app/backend/src/RealtorApi/Infrastructure/Persistence/AppDbContext.cs`.
- [X] T008 [US1] Actualizar las pruebas unitarias de configuración para verificar la PK del catálogo, la FK y el nombre de columna conservado en `app/backend/tests/RealtorApiTests/Infrastructure/Persistence/PropertyConfigurationTests.cs`.
- [X] T009 [US1] Compilar el proyecto API con `dotnet build app/backend/src/RealtorApi/RealtorApi.csproj` antes de generar la migración.
- [X] T010 [US1] Generar una única migración `AddPropertyStatusCatalog` y actualizar el snapshot en `app/backend/src/RealtorApi/Infrastructure/Migrations/`; insertar los tres estados antes de la FK, abortar ante valores históricos desconocidos y preservar propiedades y valores textuales en `Up` y `Down`.

**Checkpoint**: El catálogo y su relación quedan modelados; la migración única conserva datos y el rollback no elimina propiedades.

## Fase 4: Historia de usuario 2 - Sembrar estados y propiedades sin duplicados (Prioridad: P1)

**Objetivo**: Cargar los estados del manifiesto antes que las propiedades y resolver referencias exactas sin fallback silencioso.

**Prueba independiente**: Ejecutar dos veces el seeder y comprobar los tres valores únicos, las referencias de estado y la ausencia de propiedades duplicadas; un estado de seed desconocido produce error identificable.

### Implementación de la Historia 2

- [X] T011 [US2] Actualizar `DatabaseSeeder` para leer `propertyStatusesFile` del manifiesto, sincronizar estados por `Value` antes de las propiedades, resolver el catálogo sin fallback a `Available` y mantener equivalentes los flujos sync/async en `app/backend/src/RealtorApi/Infrastructure/Persistence/DatabaseSeeder.cs`.
- [X] T012 [US2] Extender la prueba unitaria de idempotencia para verificar los tres estados, la resolución de referencias, las ejecuciones repetidas y que un estado de seed desconocido produce un error identificable sin fallback en `app/backend/tests/RealtorApiTests/Infrastructure/Persistence/DatabaseSeederTests.cs`.

**Checkpoint**: Una segunda ejecución del seeder no duplica estados ni propiedades y no cambia estados desconocidos por defecto.

## Fase 5: Historia de usuario 3 - Consultar propiedades paginadas con imágenes (Prioridad: P1)

**Objetivo**: Exponer un listado público con `items`, metadatos completos, validación 400, orden determinista y URLs absolutas servibles.

**Prueba independiente**: Ejecutar las dos solicitudes de `RealtorApi.http`, probar parámetros inválidos y una página fuera de rango, y abrir una imagen devuelta. Verificar orden, estado textual, metadatos y coincidencia entre esquema/host de la petición y `imageUrl`.

### Implementación de la Historia 3

- [X] T013 [US3] Crear los contratos explícitos de query, elemento de respuesta y página (`items` más metadatos) en `app/backend/src/RealtorApi/Features/Properties/ListProperties/ListPropertiesModels.cs`.
- [X] T014 [P] [US3] Crear el validator del query para defaults `1`/`6`, enteros positivos y máximo `pageSize=50`, incluyendo texto no numérico, en `app/backend/src/RealtorApi/Features/Properties/ListProperties/ListPropertiesValidator.cs`.
- [X] T015 [P] [US3] Crear el mapping explícito que construye `imageUrl` absoluta con esquema y host permitidos de la petición y ruta `/assets/properties/{fileName}`; ante un nombre de archivo inválido, producir error sin respuesta parcial en `app/backend/src/RealtorApi/Features/Properties/ListProperties/ListPropertiesMapping.cs`.
- [X] T016 [P] [US3] Configurar el alias estático `/assets` hacia `wwwroot/images` sin mover assets ni quitar `/images` en `app/backend/src/RealtorApi/Program.cs`.
- [X] T017 [P] [US3] Agregar solicitudes manuales para `GET /api/properties` y `GET /api/properties?page=1&pageSize=6` en `app/backend/src/RealtorApi/RealtorApi.http`.
- [X] T018 [US3] Implementar el handler público `IHandler` con conteo, proyección `AsNoTracking`, orden `Title`/`Id`, página vacía fuera de rango, `CancellationToken` y propagación de error de mapping como HTTP 500 `ProblemDetails` sin respuesta parcial en `app/backend/src/RealtorApi/Features/Properties/ListProperties/ListPropertiesHandler.cs`.
- [X] T019 [US3] Implementar el endpoint Minimal API público como `ISlice` para `GET /api/properties`, delegando al handler y sin registro individual en `Program.cs` en `app/backend/src/RealtorApi/Features/Properties/ListProperties/ListPropertiesEndpoint.cs`; verificar su auto-descubrimiento en `app/backend/tests/RealtorApiTests/HealthEndpointTests.cs`.
- [X] T020 [US3] Agregar pruebas unitarias del validator para valores ausentes, límites, texto no numérico y valores no positivos en `app/backend/tests/RealtorApiTests/Features/Properties/ListProperties/ListPropertiesValidatorTests.cs`.
- [X] T021 [US3] Configurar `AllowedHosts` explícitos por entorno y aceptar forwarded headers solo de proxies confiables; los hosts rechazados deben responder con `ProblemDetails` y no reflejarse en `imageUrl`, en `app/backend/src/RealtorApi/Program.cs`, `app/backend/src/RealtorApi/appsettings.json` y la configuración de entorno correspondiente.
- [X] T022 [US3] Agregar pruebas unitarias del mapping y el rechazo del host para nombres de archivo inválidos, hosts no permitidos y respuesta sin resultados parciales en `app/backend/tests/RealtorApiTests/Features/Properties/ListProperties/ListPropertiesMappingTests.cs`.

**Checkpoint**: El endpoint anónimo devuelve las propiedades ordenadas con contrato paginado y URL absoluta; errores de parámetros responden 400.

## Fase 6: Pulido y validación transversal

**Propósito**: Verificar el resultado completo sin añadir alcance funcional.

- [X] T023 [US1] Agregar una prueba unitaria que inspeccione las operaciones `Down` de la migración y confirme que elimina la FK y el catálogo sin eliminar `Properties` ni su columna `Status` en `app/backend/tests/RealtorApiTests/Infrastructure/Persistence/PropertyStatusMigrationTests.cs`.
- [X] T024 Compilar el backend final con `dotnet build app/backend/src/RealtorApi/RealtorApi.csproj`.
- [X] T025 Ejecutar la suite unitaria focalizada con `dotnet test app/backend/tests/RealtorApiTests/RealtorApiTests.csproj`.
- [ ] T026 Validar migración y preservación en PostgreSQL, seeding repetido, contrato HTTP, rechazo de host, puerto `5023`, errores y serving de imágenes siguiendo `specs/004-properties-list-pagination/quickstart.md` con una base aislada.

## Dependencias y orden de ejecución

### Dependencias de fase

- **Preparación (Fase 1)**: T001 y T002 pueden ejecutarse en paralelo; ambas completan antes de las historias.
- **Fundamentos (Fase 2)**: No agrega infraestructura compartida; queda satisfecho al confirmar que los mecanismos existentes cubren el slice.
- **Historia 1 (Fase 3)**: Modelo y configuración preceden a compilación y migración; T010 depende de T003-T009.
- **Historia 2 (Fase 4)**: Depende de la entidad y migración de US1; el seeder debe consultar un catálogo ya existente.
- **Historia 3 (Fase 5)**: Depende del catálogo y del seeding de US1/US2 para que el listado integrado devuelva estados resueltos.
- **Pulido (Fase 6)**: T023-T026 dependen de las tres historias completas.

### Orden entre historias

1. **US1 → US2**: El seeder requiere la entidad, la FK y los estados canónicos disponibles.
2. **US2 → US3**: El listado integrado depende de propiedades existentes con relaciones a estados válidos.
3. **US3 → Pulido**: La validación HTTP y de assets requiere endpoint, query, mapping y ruta estática terminados.

### Oportunidades de paralelismo

- **Preparación**: T001 y T002 trabajan sobre archivos distintos.
- **US1**: Después de T003-T004, T005, T006 y T007 pueden implementarse en paralelo en archivos distintos; T008 comienza al estar definida la relación del modelo.
- **US2**: T011 y T012 pueden trabajarse en paralelo porque se separan entre seeder y pruebas.
- **US3**: Tras T013, T014, T015, T016 y T017 pueden avanzar en archivos distintos; T021 depende de T016 porque ambos modifican la configuración del host; T018 depende de los contratos, validator y mapping; T019 depende de T018; T020 depende de T013-T014; T022 depende de T015 y T021.
- **Pulido**: T023 verifica el rollback sin bajar una base activa; T024 debe completarse antes de T025; T026 requiere una base desechable independiente y se ejecuta después de las verificaciones automatizadas.

## Estrategia de implementación

### Entrega incremental

1. Completar US1 y validar el modelo/migración de forma que las filas existentes se conserven.
2. Completar US2 para que migración y seeder dejen estados y referencias idempotentes.
3. Completar US3 para entregar la consulta pública solicitada.
4. Ejecutar validación unitaria y manual según quickstart.

**MVP de datos**: US1 establece el catálogo seguro. **MVP funcional del consumidor**: requiere completar US1, US2 y US3, ya que el listado depende de estados sembrados y relacionados.

## Notas

- Cada tarea incluye checkbox, ID secuencial, etiquetas requeridas y ruta de archivo o proyecto.
- Las tareas `[P]` trabajan archivos distintos y no dependen de tareas incompletas.
- Las pruebas automatizadas se limitan a unitarias; no se generan pruebas HTTP de integración ni Testcontainers.
- La implementación y sus archivos deben permanecer trazados a estas tareas y al alcance de `spec.md`.