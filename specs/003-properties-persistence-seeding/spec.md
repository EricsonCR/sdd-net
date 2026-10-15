# Especificación: Persistencia y seeding inicial de propiedades

**Iniciativa**: `003-properties-persistence-seeding`  
**Fecha**: `2026-10-12`  
**Estado**: `Implementada`

## Contexto

Esta iniciativa define la capa de persistencia inicial para el dominio de propiedades del backend RealtorApi. Debe introducir un modelo persistente mínimo, configurable mediante `IEntityTypeConfiguration<T>`, con migración EF Core única y seeding idempotente automático al arrancar la aplicación.

La base de la solución y del backend ya existe por las specs anteriores y no debe cambiarse. La implementación debe respetar la organización de Vertical Slice y mantener la compatibilidad con infraestructura ya aprobada, sin introducir endpoints de negocio fuera de `Features` ni cambiar patrones de estructura del proyecto.

## Historia principal

### Historia 1: Persistir la base de propiedades y estados (Prioridad: P1)

Como persona desarrolladora, quiero modelar la entidad persistente de propiedad y sus estados base, para que el sistema pueda almacenar propiedades con información mínima necesaria y mantener estados consistentes.

**Prueba independiente**: una prueba unitaria valida la configuración EF Core de `Property` y la conversión de `PropertyStatus` a `string`, asegurando que el modelado y el almacenamiento respetan la especificación.

**Escenarios de aceptación**:

1. Dado un modelo `Property`, cuando se aplica la configuración EF Core, entonces la entidad queda registrada en `DbSet` correcto y usa la conversión `enum -> string` para `Status`.
2. Dado los valores de estado requeridos, cuando se inicializa el modelo, entonces `Available`, `Rented` y `Maintenance` están disponibles y almacenados como texto.
3. Dado una propiedad con los campos mínimos de la spec, cuando se persiste, entonces su esquema incluye `Id`, `Title`, `Description`, `Address`, `Price`, `Status`, `BedroomCount`, `BathroomCount`, `AreaSquareMeters`, `ImageUrl`, `CreatedAt` y `UpdatedAt`.

### Historia 2: Generar migración y esquema estable (Prioridad: P1)

Como persona desarrolladora, quiero contar con una sola migración coherente para el cambio funcional, para mantener el esquema versionado y reproducible sin fragmentar cambios por entidad.

**Prueba independiente**: una prueba o validación documental comprueba que la migración contiene el esquema esperado y que la serialización del snapshot coincide con la estructura autorizada.

**Escenarios de aceptación**:

1. Dado el modelo persistente final, cuando se genera la migración, entonces existe una sola migración para el conjunto funcional de propiedades y estados.
2. Dado la migración, cuando se revisa `Up` y `Down`, entonces refleja la creación y eliminación coherente de tablas y columnas.
3. Dado el snapshot, cuando se valida el esquema, entonces no aparecen configuraciones inline ni alteraciones fuera del alcance de la spec.

### Historia 3: Implementar seeding automático idempotente (Prioridad: P1)

Como persona desarrolladora, quiero que la base de datos se inicialice automáticamente al arrancar la aplicación, para asegurar que los datos base y los estados queden disponibles sin intervención manual ni reinsertados duplicados.

**Prueba independiente**: una prueba valida la idempotencia del seed y la ejecución de `UseSeeding` y `UseAsyncSeeding` durante el arranque del host.

**Escenarios de aceptación**:

1. Dado la base de datos vacía, cuando arranca la aplicación, entonces `app.MigrateAsync()` se ejecuta antes que el host continúe.
2. Dado el contexto de base de datos, cuando se dispara `UseSeeding` y `UseAsyncSeeding`, entonces los datos base y estados se insertan solo si no existen.
3. Dado una ejecución repetida del seed, cuando se vuelve a arrancar la app, entonces no se duplican registros ni se producen errores por clave duplicada.

### Historia 4: Integrar archivos externos y activos de imagen (Prioridad: P1)

Como persona desarrolladora, quiero que los assets de seed y las imágenes queden disponibles en runtime y consumidos por un manifiesto central, para facilitar la carga inicial de propiedades y su uso futuro en consultas de imágenes.

**Prueba independiente**: una prueba valida que el manifest central se lee correctamente, que los JSON de seed se cargan en runtime y que las imágenes se copian o sincronizan hacia la ubicación final del proyecto API.

**Escenarios de aceptación**:

1. Dado el `seed-manifest.json`, cuando la aplicación resuelve los archivos de seed, entonces usa ese manifiesto como fuente autorizada y no rutas hardcodeadas dispersas.
2. Dado la carpeta `support/seed-data`, cuando el proyecto se compila o publica, entonces los JSON y las imágenes quedan disponibles en el runtime de la API.
3. Dado una imagen del seed, cuando se expone en una propiedad, entonces su `ImageUrl` guarda la ruta pública final servida por la API, no una ruta física de `support/seed-data`.

### Historia 5: Asegurar la ruta pública de imagen y compatibilidad funcional (Prioridad: P2)

Como consumidor de la API, quiero que cada propiedad exponga una `ImageUrl` lista para devolver la imagen directamente en futuras consultas, para permitir renderizado y visualización sin necesidad de resolver rutas de filesystem.

**Prueba independiente**: una prueba valida que el `ImageUrl` generado o persistido coincide con la ruta pública final y que el asset está disponible bajo esa ruta.

**Escenarios de aceptación**:

1. Dado una propiedad cargada desde el seed, cuando se persiste su `ImageUrl`, entonces apunta a la ubicación pública final de la API.
2. Dado la carpeta de imágenes de runtime, cuando se solicita la imagen, entonces la ruta responde y corresponde al archivo correcto.
3. Dado un cambio de entorno, cuando se publica el backend, entonces la ruta pública sigue siendo estable y consumible.

## Casos límite

- La base de datos puede estar vacía, ya creada o parcialmente inicializada; el seeding debe ser idempotente.
- No debe haber más de una migración para este cambio funcional. Si se requiere una revisión de schema, debe hacerse dentro de una sola migración coherente.
- Si no hay migraciones pendientes, el arranque de la aplicación no debe dejar de ejecutar `UseAsyncSeeding`.
- Los archivos externos deben resolverse desde `seed-manifest.json` y no por rutas absolutas dispersas.
- Las imágenes deben tratarse como assets públicos del backend y no como archivos del sistema de desarrollo.

## Requisitos

### Requisitos funcionales

- **FR-001**: El backend DEBE mantener la arquitectura Vertical Slice y no crear una jerarquía paralela de endpoints de negocio fuera de `Features`.
- **FR-002**: La iniciativa DEBE introducir `PropertyStatus` con los valores `Available`, `Rented` y `Maintenance`.
- **FR-003**: La iniciativa DEBE introducir la entidad persistente `Property` con al menos los campos requeridos por la spec: `Id`, `Title`, `Description`, `Address`, `Price`, `Status`, `BedroomCount`, `BathroomCount`, `AreaSquareMeters`, `ImageUrl`, `CreatedAt` y `UpdatedAt`.
- **FR-004**: `Status` DEBE almacenarse como `string` mediante conversión EF Core y no como entero o enum serializado directamente.
- **FR-005**: El modelo DEBE configurarse mediante `IEntityTypeConfiguration<T>` en `Infrastructure/Persistence/Configurations` y `AppDbContext` debe registrar los `DbSet` necesarios.
- **FR-006**: `OnModelCreating` DEBE aplicar las configuraciones por ensamblado y no incluir mapping inline dentro del método.
- **FR-007**: La especificación DEBE generar una sola migración coherente para el cambio funcional de propiedades y estados.
- **FR-008**: El arranque de la API DEBE ejecutar siempre `app.MigrateAsync()` antes de `app.Run()`.
- **FR-009**: El contexto de base de datos DEBE configurar `UseSeeding` y `UseAsyncSeeding` en la inicialización del host.
- **FR-010**: `DatabaseSeeder` DEBE implementar versión sync y async equivalentes y debe ser idempotente.
- **FR-011**: El seeding NO DEBE invocarse manualmente desde `MigrationExtensions` ni como fallback manual del arranque.
- **FR-012**: El backend DEBE resolver todos los archivos de seed desde `support/seed-data/seed-manifest.json` y no desde rutas hardcodeadas dispersas.
- **FR-013**: El proyecto DEBE integrar los JSON, el manifest y las imágenes al `RealtorApi.csproj` para que queden disponibles en runtime y publish.
- **FR-014**: `ImageUrl` DEBE persistir la ruta pública final servida por la API, nunca rutas físicas del sistema de archivos ni rutas de `support/seed-data`.
- **FR-015**: La implementación DEBE ser compatible con futuros procesos de inserción de propiedades y con consultas de propiedades con imagen.
- **FR-016**: Deben existir pruebas que verifiquen: configuración EF del modelo y conversión enum-string; presencia del historico de migración esperado; idempotencia del seed; ejecución async del seed en arranque; lectura del JSON y manifest; copia/sincronización de imágenes y consistencia de `ImageUrl`.
- **FR-017**: No se deben usar controllers, `HasData` ni migraciones internas de esquema por cada entidad.
- **FR-018**: Se prohibe la creación de una migración por cada entidad si forman parte del mismo cambio funcional.
- **FR-019**: El proyecto DEBE soportar un manifiesto central con rutas relativas a `support/seed-data`, además de una política de publicación que deje accesibles los assets de imagen en runtime.
- **FR-020**: El flujo de arranque DEBE respetar el orden requerido: `MigrateAsync()` → `UseAsyncSeeding()` → `SeedAsync()` en el host de la aplicación.

### Requisitos no funcionales

- **NFR-001**: La implementación debe usar EF Core y PostgreSQL con Npgsql conforme al stack del repositorio.
- **NFR-002**: La solución debe seguir el patrón de arquitectura y convención del proyecto: `Domain`, `Features`, `Infrastructure` y `Persistence` bajo infraestructura.
- **NFR-003**: El código debe ser compilable con el SDK del repositorio y el proyecto no debe depender de rutas relativas de desarrollo que rompan en publish o despliegue.
- **NFR-004**: Las pruebas deben ser deterministas, idempotentes y no depender de contenido temporal fuera del repositorio.

## Entidades clave

### `PropertyStatus`

Enumerado persistido como `string` en base de datos.

Valores:

- `Available`
- `Rented`
- `Maintenance`

### `Property`

Entidad persistente base con los siguientes campos mínimos:

- `Id`: identificador único.
- `Title`: título visible de la propiedad.
- `Description`: descripción de la propiedad.
- `Address`: ubicación exacta.
- `Price`: precio de la propiedad.
- `Status`: estado actual; serializado como texto.
- `BedroomCount`: número de dormitorios.
- `BathroomCount`: número de baños.
- `AreaSquareMeters`: superficie total.
- `ImageUrl`: ruta pública final para la imagen servida por la API.
- `CreatedAt`: marca temporal de creación.
- `UpdatedAt`: última actualización, nullable o no según decisión del diseño final.

## Criterios de éxito

- **SC-001**: El modelo persistente de `Property` y `PropertyStatus` compila y queda configurado con `IEntityTypeConfiguration<T>`.
- **SC-002**: La migración única generada para la spec refleja el esquema esperado y queda revisada antes de cerrar la implementación.
- **SC-003**: El seeding se ejecuta automáticamente con `UseSeeding` y `UseAsyncSeeding`, sin duplicar registros y sin depender de ejecuciones manuales.
- **SC-004**: Los assets de `support/seed-data` se publican con el proyecto y quedan consumibles en runtime.
- **SC-005**: Las imágenes se resuelven a una ruta pública final y `ImageUrl` no contiene rutas físicas de `support`.
- **SC-006**: Las pruebas de configuración, migración, seeding e imágenes pasan y documentan la evidencia de cumplimiento.

## Fuera de alcance

- Crear endpoints de negocio distintos de la persistencia base y el seed.
- Implementar consultas complejas de propiedades que no estén directamente relacionadas con la persistencia inicial.
- Crear una arquitectura de repositorio o servicio adicional no previsto por la persistence base.
- Diseñar flujos de autenticación, autorización o front-end.
- Generar múltiples migraciones por entidad dentro de la misma iniciativa.

## Suposiciones

- La base de datos será PostgreSQL con Npgsql, conforme al stack definido para el repositorio.
- Los archivos externos de seed ya existen bajo `support/seed-data` o deben añadirse antes de la implementación del seeder.
- La ruta pública de imágenes se basa en el runtime del backend y debe ser revisada en la configuración específica de `wwwroot` y `UseStaticFiles`.
- La iniciativa define la base funcional para futuras inserciones y consultas de propiedades con imagen; no reemplaza un flujo completo de ventas o gestión de inmuebles.
