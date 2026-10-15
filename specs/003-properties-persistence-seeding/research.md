# Investigación técnica: propiedades y seeding

## Objetivo

Definir la solución técnica para la persistencia inicial de propiedades: modelo de dominio, EF Core, migración, seeding idempotente y assets de runtime para imágenes.

## Decisiones técnicas

### 1. Modelo persistente

La entidad `Property` se modelará con los campos mínimos de la spec. `Status` será un enum del dominio, pero persistido como `string` utilizando conversión de EF Core. Esto permite consultar y filtrar por string sin perder legibilidad de la base de datos y evita la necesidad de un tipo entero o un valor sintáctico ambiguo.

### 2. Configuración EF Core

La entidad y sus constraints quedarán bajo `Infrastructure/Persistence/Configurations` con `IEntityTypeConfiguration<T>`. Esto mantiene la separación de responsabilidades y respeta la política del repositorio de no usar `OnModelCreating` para inline mapping.

### 3. Migración única

Se generará una sola migración para la base funcional de propiedades. Esto es más estable, más fácil de revisar y más consistente con la regla del repositorio que prohíbe migraciones por entidad dentro de la misma iniciativa.

### 4. Seeding idempotente

El seeder se implementará en dos versiones equivalentes: sync y async. El `DbContext` estará configurado con `UseSeeding` y `UseAsyncSeeding` según la convención del proyecto. La semántica debe verificar si ya existen registros antes de insertar, evitando duplicados y logrando ejecución repetida segura.

### 5. Runtime de assets y rutas públicas

El proyecto debe publicar el contenido de `support/seed-data` para runtime. Las imágenes se copiarán o sincronizarán hacia una carpeta de assets del backend, por ejemplo `wwwroot/images/properties`. `ImageUrl` almacenará esa ruta pública final, ejemplo: `/images/properties/1.png`, que puede devolverse directamente en consultas futuras sin depender del filesystem de desarrollo.

### 6. Manifest central

Se definirá un `seed-manifest.json` que enumere las rutas relativas a los JSON de estado y propiedades, además del origen y destino de las imágenes. Esto centraliza la configuración y evita rutas hardcodeadas dispersas.

## Trade-offs evaluados

- Se prefirió conversión de `enum` a `string` para facilitar legibilidad, exportación y compatibilidad de SQL.
- Se prefirió un único seed sincronizado por manifest antes que rutas dispersas de archivos. Esto reduce errores de despliegue.
- Se prefirió un diseño basado en `UseSeeding` / `UseAsyncSeeding` en vez de invocarlo manualmente desde `MigrationExtensions` para cumplir la secuencia requerida por la base de datos.

## Riesgos y mitigaciones

- Riesgo: rutas relativas rotas en publish. Mitigación: usar manifest central y validar copia al destino de runtime.
- Riesgo: duplicados en plantillas de seed. Mitigación: revisar por clave natural y aplicar idempotencia.
- Riesgo: migración incompleta. Mitigación: revisar `Up`, `Down` y snapshot antes de cerrar la tarea.
