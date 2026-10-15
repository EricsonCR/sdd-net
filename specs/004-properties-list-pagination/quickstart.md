# Validación manual: Catálogo y listado de propiedades

**Iniciativa**: `004-properties-list-pagination`  
**Contrato**: [contracts/properties-list.md](contracts/properties-list.md)  
**Modelo**: [data-model.md](data-model.md)

Esta guía verifica el catálogo, la preservación de datos, el endpoint público y la ruta de imágenes. No ejecuta ni sustituye pruebas automatizadas.

## Prerrequisitos

- SDK .NET `10.0.401`, según `global.json`.
- PostgreSQL accesible para la API y una base de datos aislada para la validación.
- En el escenario de preservación, una copia desechable de una base con el esquema de la iniciativa 003 y propiedades existentes. No ejecutar esta validación sobre una base de datos de usuario sin respaldo.
- PowerShell desde la raíz del repositorio; este procedimiento inicia la API en `http://localhost:5023` para comprobar el host exacto del criterio SC-005.

## Comprobaciones automatizadas unitarias

```powershell
dotnet test app/backend/tests/RealtorApiTests/RealtorApiTests.csproj
```

Resultado esperado: compilación y pruebas unitarias completadas sin fallos. La suite no sustituye la verificación con PostgreSQL descrita abajo.

## Arranque con PostgreSQL

Configure una cadena de conexión local real en el entorno de la terminal. Sustituya el valor de contraseña por el valor local, sin guardarlo en los archivos del repositorio:

```powershell
$env:ConnectionStrings__DefaultConnection = "Host=localhost;Port=5432;Database=RealtorApi_004;Username=postgres;Password=<contraseña-local>"
$env:AllowedHosts = "localhost;127.0.0.1"
dotnet run --no-launch-profile --urls http://localhost:5023 --project app/backend/src/RealtorApi/RealtorApi.csproj
```

Con una conexión válida, el arranque aplica las migraciones pendientes y ejecuta el seeding idempotente mediante EF Core. La API escucha en `http://localhost:5023`. En otros entornos, `AllowedHosts` debe enumerar explícitamente los hosts públicos; no usar `*`. Si la API recibe tráfico de un proxy administrado, configure únicamente sus IPs como confiables; por ejemplo, para el primer proxy:

```powershell
$env:ForwardedHeaders__KnownProxies__0 = "10.0.0.10"
```

No configure esa variable si no hay un proxy conocido delante de la API.

## Verificar el catálogo y la conservación de datos

En la base aislada que ya contiene datos de 003, registra antes del arranque el total de propiedades y una muestra de sus `Id`, `Status` y campos principales. Después de que la aplicación aplique la migración, comprueba con `psql`:

```sql
SELECT "Value", "Description"
FROM property_status
ORDER BY "Value";

SELECT COUNT(*) AS propiedades_sin_estado
FROM "Properties" AS p
LEFT JOIN property_status AS s ON s."Value" = p."Status"
WHERE s."Value" IS NULL;

SELECT COUNT(*) AS total_propiedades
FROM "Properties";
```

Resultado esperado: una fila para cada estado canónico, cero propiedades sin estado y el mismo total/valores de propiedades registrados antes de migrar. Repite el seeding reiniciando la API: los estados y propiedades no deben duplicarse.

## Verificar el endpoint

```powershell
curl.exe -i "http://localhost:5023/api/properties"
curl.exe -i "http://localhost:5023/api/properties?page=2&pageSize=6"
curl.exe -i "http://localhost:5023/api/properties?page=abc&pageSize=6"
curl.exe -i -H "Host: not-allowed.invalid" "http://localhost:5023/api/properties"
```

Resultados esperados:

- Primera solicitud: `200`, `page=1`, `pageSize=6`, hasta seis elementos en orden de título ascendente, `items` y todos los metadatos.
- Segunda solicitud: `200`, página dos con metadatos coherentes; una página posterior a `totalPages` devuelve `items: []`.
- Tercera solicitud: `400` con `ValidationProblemDetails` que identifica el parámetro `page`.
- Cuarta solicitud: `400` con `ProblemDetails`; `not-allowed.invalid` no debe aparecer en ningún enlace.
- Ejecutar la primera solicitud sin encabezado `Authorization` confirma el acceso anónimo.
- Cada elemento muestra el valor textual de estado (`Available`, `Rented` o `Maintenance`) y una URL absoluta `http://localhost:5023/assets/properties/{fileName}`.

## Verificar una imagen

Solicite una de las URLs exactas devueltas por el endpoint. Con los datos seed actuales, una muestra es:

```powershell
curl.exe -i "http://localhost:5023/assets/properties/1.png"
```

Resultado esperado: `200` y contenido de imagen. Esta comprobación usa el puerto exacto `5023` definido por SC-005.

## Verificar rollback de migración

La prueba unitaria de migración verifica las operaciones `Down`: elimina la FK y `property_status`, pero no elimina `Properties` ni su columna `Status`. No se debe bajar la base y arrancar la aplicación de 004 contra el esquema 003, porque ese binario requiere el catálogo. La validación manual de `Up` y preservación se realiza únicamente en la base aislada descrita arriba.

## Referencias

- Contrato: [properties-list.md](contracts/properties-list.md).
- Entidades y reglas: [data-model.md](data-model.md).