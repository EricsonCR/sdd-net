# Plan de implementación: Fundación de la solución Realtor

**Rama**: `001-solution-foundation` | **Fecha de actualización**: `2026-10-12` | **Spec**: `specs/001-solution-foundation/spec.md`

**Entrada**: Especificación de `specs/001-solution-foundation/spec.md`

## Resumen

La iniciativa establece una solución única compilable con backend Minimal API, frontend Blazor Web App y sus proyectos de pruebas, sin lógica de negocio ni entidades de dominio. El endpoint `GET /health` verifica el arranque y conserva el contrato HTTP definido para la solución.

## Contexto técnico

**Lenguaje y versión**: C# / .NET 10; SDK `10.0.401` fijado en `global.json` en la raíz del repositorio.

**Dependencias principales**: ASP.NET Core Minimal APIs, Blazor Web App, xUnit 2.9.3, Microsoft.NET.Test.Sdk 17.14.1 y Microsoft.AspNetCore.Mvc.Testing 10.0.0 para la prueba de `/health`.

**Almacenamiento**: No aplica. PostgreSQL y EF Core quedan fuera de esta iniciativa.

**Pruebas**: xUnit para los proyectos backend y frontend. Las pruebas son unitarias, con una única prueba HTTP en proceso mediante `WebApplicationFactory` para comprobar `GET /health`. Esta excepción se limita a la sonda y requiere que la spec esté aprobada y que su justificación conste también en las tareas antes de considerarse habilitada para implementación.

**Plataforma objetivo**: Desarrollo local y compilación en Windows, Linux y macOS con .NET 10.

**Tipo de proyecto**: Aplicación web con backend, frontend y pruebas automatizadas.

**Objetivos de rendimiento**: No se definen objetivos de rendimiento de producto; se requiere una compilación consistente y un endpoint de salud local.

**Restricciones**: .NET 10 según la constitución y la spec; no modificar `global.json`; no usar controllers; no agregar lógica de negocio, entidades, migraciones ni persistencia de producto. Las instrucciones backend mencionan .NET 11, en conflicto con la constitución; prevalece la versión .NET 10 fijada por la constitución y `global.json`.

**Escala y alcance**: Estructura base de la solución, sin casos de uso, dominio ni integraciones externas.

## Verificación de la constitución

*Puerta de control: verificar antes de la investigación y volver a comprobar después del diseño.*

- ✅ Mantiene una única solución con backend, frontend y proyectos de pruebas.
- ✅ Sigue el stack establecido por la constitución y la spec: .NET 10, Minimal APIs y Blazor Web App; la versión se resuelve sin modificar `global.json`.
- ✅ No usa controllers ni introduce dominio, persistencia o funcionalidad de producto.
- ✅ La respuesta de salud no depende de servicios externos ni de persistencia.
- ⚠️ La prueba `WebApplicationFactory` es una excepción limitada a `/health`. La aclaración está registrada en la spec; para habilitar la excepción conforme a la constitución, la spec debe pasar a Aprobada y la tarea T017 debe nombrar expresamente el alcance de la prueba antes de iniciar una nueva implementación.
- ✅ La política de artefactos auxiliares está sincronizada en la enmienda constitucional 1.1.0; los documentos de diseño quedan referenciados en este plan.

### Revisión posterior al diseño

- ✅ El diseño conserva el alcance de la spec: solución existente, sin cambios de SDK, dominio, persistencia ni funcionalidades de producto.
- ✅ El modelo de datos y el contrato de salud existentes siguen alineados; esta aclaración no requiere modificarlos.
- ✅ `research.md`, `data-model.md`, `quickstart.md` y `contracts/health-endpoint.md` son las salidas auxiliares permitidas y están enumeradas en este plan.
- ⏳ La excepción de `WebApplicationFactory` queda limitada y justificada, pero no habilita una nueva implementación hasta que la spec esté Aprobada y T017 identifique expresamente la prueba.

## Estructura del proyecto

### Documentación de esta iniciativa

```text
specs/001-solution-foundation/
├── spec.md                 # Artefacto canónico
├── plan.md                 # Artefacto canónico y referencia a salidas auxiliares
├── tasks.md                # Artefacto canónico
├── research.md             # Salida auxiliar de speckit.plan
├── data-model.md           # Salida auxiliar de speckit.plan
├── quickstart.md           # Salida auxiliar de speckit.plan
├── contracts/
│   └── health-endpoint.md  # Salida auxiliar de speckit.plan
```

### Código fuente (raíz del repositorio)

```text
global.json
app/
├── Realtor.sln
├── backend/
│   ├── src/
│   │   └── RealtorApi/
│   │       ├── Program.cs
│   │       ├── Properties/
│   │       └── appsettings*.json
│   └── tests/
│       └── RealtorApiTests/
│           └── HealthEndpointTests.cs
├── frontend/
│   ├── src/
│   │   └── RealtorWeb/
│   │       ├── Program.cs
│   │       ├── Components/
│   │       ├── wwwroot/
│   │       └── app.css
│   └── test/
│       └── RealtorWeb/
│           ├── SmokeTests.cs
│           └── UnitTest1.cs
```

**Decisión de estructura**: Se conserva la estructura definida por la spec: `app/Realtor.sln` y los cuatro proyectos en sus rutas aprobadas. `global.json` permanece en la raíz del repositorio. Los artefactos auxiliares enumerados arriba son las únicas salidas adicionales de planificación permitidas por la constitución.

## Registro de complejidad

| Excepción | Justificación | Límite |
|---|---|---|
| Una prueba con `WebApplicationFactory` para `/health` | Verificar que la aplicación inicia, descubre la ruta y responde conforme al contrato requiere atravesar el host HTTP. | Solo `/health`; no autoriza otras pruebas de integración. La spec debe estar Aprobada y T017 debe identificar la excepción antes de habilitar una nueva implementación. |
