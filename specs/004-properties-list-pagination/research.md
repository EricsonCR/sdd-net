# Investigación: Catálogo de estados y listado paginado de propiedades

**Iniciativa**: `004-properties-list-pagination`  
**Fecha**: `2026-10-15`  
**Spec**: [spec.md](spec.md)

Las cuatro decisiones pendientes de producto/modelo se resolvieron durante `speckit.clarify`: clave natural textual `Value`, máximo `pageSize` de 50, colección `items` y acceso anónimo. No quedan marcadores de aclaración pendientes.

## Decisión 1: Relación y migración del catálogo

**Decisión**: Reemplazar el enum persistente por una entidad `PropertyStatus` con `Value` textual como clave primaria y `Description` requerida. Mantener `Properties.Status` como texto y convertirla en FK a `property_status.Value`. Crear el catálogo y sus tres registros canónicos dentro de la única migración; comprobar valores históricos desconocidos y abortar antes de agregar la FK. En `Down`, retirar la FK y el catálogo, dejando intacta la columna textual y las propiedades.

**Justificación**: La migración vigente almacena `Property.Status` como cadena de hasta 32 caracteres. Mantener el mismo tipo y valor evita reescritura, pérdida o traducción de las filas. El catálogo debe estar poblado antes de agregar la FK; EF Core ejecuta el seeding configurado después de aplicar migraciones, por lo que el seeder no puede ser el primer paso que satisfaga esa FK.

**Alternativas consideradas**:

- Usar UUID o entero como clave primaria requeriría agregar/rellenar otra columna y cambiar la FK sin aportar valor al contrato aprobado.
- Convertir a entero, recrear la tabla o sustituir estados desconocidos por `Available` viola la preservación de datos y la spec.
- Crear primero la FK y esperar al seeder para insertar estados falla para cualquier base con propiedades existentes.
- Borrar `Properties` en `Down` perdería datos y está explícitamente prohibido.

**Evidencia local**: [PropertyConfiguration.cs](../../app/backend/src/RealtorApi/Infrastructure/Persistence/Configurations/PropertyConfiguration.cs), [migración 003](../../app/backend/src/RealtorApi/Infrastructure/Migrations/20261012105814_AddPropertyPersistence.cs), [database.instructions.md](../../.github/instructions/database.instructions.md).

## Decisión 2: Seeding de estados y propiedades

**Decisión**: Leer el catálogo desde `propertyStatusesFile` del `seed-manifest.json`; asegurar por `Value` exacto cada estado declarado antes de procesar propiedades; usar las descripciones canónicas del archivo de estados; resolver la propiedad contra la fila de catálogo y fallar con error identificable si no existe. Conservar la idempotencia actual por `Property.Id`. Mantener `Seed` y `SeedAsync` delegando en la misma lógica de `DatabaseSeeder` y propagar el token en operaciones asíncronas.

**Justificación**: El manifiesto vigente apunta a `properties-statuses.json`, que contiene `Available`, `Rented` y `Maintenance` con sus descripciones. El seeder ya es la ubicación central y los callbacks `UseSeeding`/`UseAsyncSeeding` ya están registrados. El seeder actual no carga el catálogo y reemplaza estados no reconocidos silenciosamente con `Available`, lo que debe corregirse.

**Alternativas consideradas**:

- `HasData` o inserciones directas en `Program.cs` duplicarían mecanismos y contradicen las reglas de persistencia.
- Leer `properties-status.json` ignoraría la ruta plural declarada por el manifiesto.
- Fallback a `Available` oculta datos inválidos y rompe la relación esperada.
- Modificar propiedades existentes durante el seed no es necesario para la idempotencia y podría sobrescribir datos de usuario.

**Evidencia local**: [DatabaseSeeder.cs](../../app/backend/src/RealtorApi/Infrastructure/Persistence/DatabaseSeeder.cs), [seed-manifest.json](../../support/seed-data/seed-manifest.json), [properties-statuses.json](../../support/seed-data/properties-statuses.json), [database-migration-and-seeding/SKILL.md](../../.github/skills/database-migration-and-seeding/SKILL.md).

## Decisión 3: Binding, validación y consulta paginada

**Decisión**: Mantener un request de query explícito cuyos valores `page` y `pageSize` se reciben como texto nullable, con defaults aplicados al normalizar la entrada. Validarlos mediante FluentValidation para que valores ausentes, no enteros, no positivos o `pageSize > 50` produzcan `ValidationProblemDetails` HTTP 400. El handler público del caso de uso implementa `IHandler` porque el scanner actual descubre solo tipos públicos; accederá a `AppDbContext`, ejecutará conteo y página con `AsNoTracking`, proyección EF directa, `OrderBy(Title).ThenBy(Id)`, `Skip`/`Take` y `CancellationToken`.

La página fuera del total se resuelve como colección vacía. Antes de calcular un offset entero, el handler compara la página con `totalPages`, evitando desbordamiento para páginas válidas muy grandes. Las páginas vacías tienen `totalPages = 0`; `hasPrevious` es `page > 1` según la spec.

**Justificación**: El filtro global valida únicamente parámetros que el proveedor HTTP logró enlazar y para los cuales existe `IValidator<T>`. Binding directo a `int` puede rechazar texto no numérico antes de construir el request y fuera del filtro. Binding a texto permite que el validator responda de forma uniforme con los errores contractuales. La proyección y el orden en base de datos evitan cargar entidades y hacen la paginación determinista.

**Alternativas consideradas**:

- Query params `int` no satisfacen uniformemente el contrato de error para valores no numéricos.
- Ejecutar `Skip` antes de comprobar página y total arriesga overflow y no aporta datos para páginas fuera de rango.
- Materializar todas las propiedades y paginar en memoria aumenta consumo y contradice la lectura proyectada.
- Controller, registro manual o scanner adicional duplica infraestructura existente.
- Handler `internal` no sería registrado por el scanner actual.

**Evidencia local**: [ValidationFilterFactory.cs](../../app/backend/src/RealtorApi/Infrastructure/Validation/ValidationFilterFactory.cs), [HandlerRegistrationExtensions.cs](../../app/backend/src/RealtorApi/Infrastructure/Handlers/HandlerRegistrationExtensions.cs), [vertical-slice-handlers/SKILL.md](../../.github/skills/vertical-slice-handlers/SKILL.md), [vertical-slice-mapping/SKILL.md](../../.github/skills/vertical-slice-mapping/SKILL.md).

## Decisión 4: URLs de imagen y serving público

**Decisión**: Mantener los archivos en `wwwroot/images/properties` y configurar una ruta estática adicional `/assets` sobre `wwwroot/images`. El mapping toma un nombre de archivo utilizable del valor persistido, forma `/assets/properties/{fileName}` y construye la URL absoluta solo con un esquema y host validados. La configuración de cada entorno debe definir hosts permitidos sin `*`; los forwarded headers de host/esquema solo se aceptan de proxies explícitamente confiables. Un host no permitido responde HTTP 400 con `ProblemDetails`. Si el nombre de archivo no es utilizable, el endpoint responde HTTP 500 con `ProblemDetails` y no entrega resultados parciales. El response final no reutiliza `publicBasePath` como autoridad ni incluye rutas físicas.

**Justificación**: El manifiesto ya copia las imágenes a `wwwroot/images/properties` y `Program.cs` las sirve actualmente bajo `/images`. La ruta adicional satisface el nuevo contrato sin mover archivos ni romper la ruta previa. La URL absoluta solo puede completarse con el contexto HTTP y por eso se construye después de materializar la proyección de datos.

**Alternativas consideradas**:

- Mover assets o cambiar `ImageDestinationFolder` amplía el alcance y altera la base funcional de 003.
- Devolver la ruta relativa persistida no satisface el contrato absoluto.
- Configurar una URL base fija no respeta el host y puerto de la solicitud actual.
- Incluir la lógica de URL en una proyección SQL no es traducible y mezcla mapping HTTP con persistencia.

**Justificación de seguridad**: `appsettings.json` actualmente permite cualquier host. Reflejar `Request.Host` sin validación podría devolver enlaces controlados por quien envíe la solicitud. La implementación debe usar host filtering con configuración explícita y no confiar en forwarded headers sin configurar los proxies autorizados. Los dominios concretos siguen siendo configuración de cada entorno.

**Alternativas consideradas**:

- Aceptar cualquier host (`AllowedHosts: "*"`) permite reflejar un host arbitrario y se descarta.
- Usar una URL base fija no cumple el contrato de host y puerto de la petición.
- Devolver `null`, una URL ficticia o resultados parciales ante una imagen inválida incumple el contrato no nulo y oculta datos corruptos; se responde con error identificable.

**Evidencia local**: [Program.cs](../../app/backend/src/RealtorApi/Program.cs), [seed-manifest.json](../../support/seed-data/seed-manifest.json), [PropertyImageAssetTests.cs](../../app/backend/tests/RealtorApiTests/Infrastructure/Persistence/PropertyImageAssetTests.cs).

## Decisión 5: Framework y estrategia de verificación

**Decisión**: Usar .NET 10 y SDK `10.0.401`; limitar pruebas automatizadas a unitarias xUnit. Probar manualmente el flujo real PostgreSQL/HTTP mediante el quickstart y las dos solicitudes en `RealtorApi.http`.

**Justificación**: La constitución y `global.json` fijan .NET 10 y los proyectos usan `net10.0`; la instrucción de backend que dice .NET 11 contradice esos documentos y se subordina a la constitución. La constitución solo permite pruebas unitarias salvo justificación explícita de otras. El skill de pruebas exige aprobación explícita de pruebas de integración; no se añadirá esa suite.

**Alternativas consideradas**:

- .NET 11 contradice la constitución y el SDK instalado para el repositorio.
- `WebApplicationFactory` o Testcontainers automatizados no están aprobados como tipo de prueba en los artefactos vigentes.
- `SQLite` no sustituye PostgreSQL para verificar comportamiento de migración y FK.

**Evidencia local**: [constitution.md](../../.specify/memory/constitution.md), [global.json](../../global.json), [RealtorApi.csproj](../../app/backend/src/RealtorApi/RealtorApi.csproj), [RealtorApiTests.csproj](../../app/backend/tests/RealtorApiTests/RealtorApiTests.csproj), [testing-minimal-apis/SKILL.md](../../.github/skills/testing-minimal-apis/SKILL.md), [run-tests/SKILL.md](../../.github/skills/run-tests/SKILL.md).

## Discrepancias existentes

- `Program.cs` llama `app.MigrateAsync()` solo si hay una conexión real; las instrucciones de persistencia indican que el flujo central se ejecute al iniciar. Esta iniciativa no cambia ese comportamiento preexistente porque no lo requiere la spec; el quickstart debe usar una conexión real PostgreSQL para activar las migraciones.
- El directorio de migraciones existente es `Infrastructure/Migrations`, aunque una instrucción de persistencia muestra `Infrastructure/Persistence/Migrations`. Se conserva la ubicación ya usada y descubierta por el proyecto.
- `.github/skills/ef-core-entity-configration/SKILL.md` existe vacío; la configuración seguirá las instrucciones de base de datos y el patrón local de `PropertyConfiguration`.