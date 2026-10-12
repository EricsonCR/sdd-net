# Guía rápida de validación

## Prerrequisitos

- SDK .NET `10.0.401`, seleccionado por `global.json` en la raíz del repositorio.
- Paquetes restaurables desde las fuentes configuradas.
- Ningún servicio externo ni base de datos es necesario.

## Restaurar y compilar

Desde la raíz del repositorio:

```powershell
dotnet --version
dotnet restore app/Realtor.sln
dotnet build app/Realtor.sln --no-restore
```

Resultado esperado: la versión reportada es `10.0.401` y la solución compila sin errores.

## Pruebas del backend

La spec debe haber sido aprobada antes de iniciar la implementación; esa aprobación y su transición a **En implementación** quedan registradas en `spec.md`. T017 autoriza expresamente la única prueba HTTP, limitada a `/health`.

```powershell
dotnet test app/backend/tests/RealtorApiTests/RealtorApiTests.csproj
```

Resultado esperado: pasan las pruebas unitarias de infraestructura y la prueba HTTP en proceso de `GET /health`. No se habilitan pruebas de host para otras rutas ni servicios externos.

## Comprobar `/health` manualmente

Inicia la API:

```powershell
dotnet run --project app/backend/src/RealtorApi
```

Usa la URL de escucha indicada por el host:

```powershell
curl.exe -i http://localhost:<puerto>/health
```

Resultado esperado: HTTP `200 OK` y JSON con `status: "Healthy"`, `service: "RealtorApi"` y `timestamp`, conforme a `contracts/health-endpoint.md`.

## Comprobación de alcance

La revisión final confirma que el backend no contiene entidades, casos de uso de negocio, persistencia, migraciones, seeders ni conexiones a base de datos.

## Evidencia de validación

**Fecha**: 2026-10-12

- `dotnet --version`: `10.0.401`.
- `dotnet restore app/Realtor.sln`: completado correctamente.
- `dotnet build app/Realtor.sln --no-restore`: completado correctamente; compilaron los cuatro proyectos.
- `dotnet test app/backend/tests/RealtorApiTests/RealtorApiTests.csproj`: 7 pruebas pasaron, 0 fallaron.
- La prueba de `GET /health` confirmó el descubrimiento de una `ISlice`, HTTP 200, JSON y los campos `status`, `service` y `timestamp`.
- La revisión de producción encontró únicamente la ruta de infraestructura `/health`, sin controllers, persistencia ni endpoints de negocio.