# Quickstart: persistencia y seeding de propiedades

## Objetivo

Validar, de forma reproducible, que la base funcional de propiedades y estados queda correctamente modelada, migrada y sembrada sin romper la solución existente.

## Verificación recomendada

1. Restaurar dependencias:

```bash
dotnet restore app/Realtor.sln
```

2. Compilar la solución:

```bash
dotnet build app/Realtor.sln --nologo
```

3. Ejecutar pruebas del backend:

```bash
dotnet test app/backend/tests/RealtorApiTests/RealtorApiTests.csproj --nologo -p:UseAppHost=false
```

> En Windows, esta validación usa `-p:UseAppHost=false` para evitar el bloqueo del archivo `RealtorApi.exe` por el host de la app. La lógica de la feature sigue validándose con la suite enfocada del backend.

4. Validar el seeding y la resolución de runtime:

- Confirmar que `seed-manifest.json` se carga desde `support/seed-data`.
- Confirmar que `properties.json` y `properties-statuses.json` se leen sin rutas hardcodeadas.
- Confirmar que las imágenes se copian a `wwwroot/images/properties` y que `ImageUrl` apunta a `/images/properties/...`.

## Validaciones de migración

La migración debe revisarse antes de cerrar la tarea:

- `Up`: crea esquema y columnas requeridas.
- `Down`: elimina la tabla y columnas con reversibilidad.
- Snapshot: refleja el modelado sin inline mappings.

## Excepciones

- No se exige `dotnet ef database update` como criterio obligatorio de cierre.
- La validación de arranque debe centrarse en `MigrateAsync()`, `UseSeeding`, `UseAsyncSeeding` y la idempotencia del `DatabaseSeeder`.
