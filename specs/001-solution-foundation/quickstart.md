# Guía rápida de validación

## Prerrequisitos

1. Instalar el SDK .NET 10.0.401 indicado por `global.json`.
2. Tener acceso a una terminal con permisos para ejecutar `dotnet`.
3. Clonar o abrir el repositorio en la carpeta base del proyecto.

## Validación de compilación

Ejecuta lo siguiente desde la raíz del repositorio:

```bash
dotnet --version
dotnet build app/Realtor.sln
```

### Resultado esperado

- `dotnet --version` reporta la versión `10.0.401`.
- `dotnet build app/Realtor.sln` termina con éxito.
- Los cuatro proyectos de la solución se compilan sin errores.

## Validación del endpoint de salud

1. Inicia la API desde la raíz del repositorio:

```bash
dotnet run --project app/backend/src/RealtorApi
```

2. Espera a que el host indique el puerto de escucha.
3. Ejecuta una petición HTTP al endpoint de salud:

```bash
curl http://localhost:<puerto>/health
```

### Resultado esperado

- Código HTTP: `200 OK`.
- Cuerpo JSON con un estado saludable, por ejemplo:

```json
{
  "status": "Healthy",
  "service": "RealtorApi",
  "timestamp": "2026-10-10T00:00:00.0000000Z"
}
```

## Verificación final

La feature se considera validada cuando:

- La solución compila íntegramente.
- El backend arranca correctamente.
- El endpoint `/health` responde con éxito.
- No se han añadido entidades de negocio ni lógica de producto.
