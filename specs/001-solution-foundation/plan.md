# Implementation Plan: Fundación de la solución Realtor

**Branch**: `001-solution-foundation` | **Date**: `2026-10-10` | **Spec**: `/specs/001-solution-foundation/spec.md`

**Input**: Feature specification from `/specs/001-solution-foundation/spec.md`

## Summary

La iniciativa crea la base de la solución Realtor como un único sistema con backend, frontend y suites de pruebas. El objetivo principal es dejar una solución compilable, con el SDK fijado por `global.json`, sin introducir lógica de negocio ni entidades del dominio. La capa de API se implementa con Minimal APIs y el endpoint `GET /health` como comprobación de arranque mínimo.

## Technical Context

**Language/Version**: C# / .NET 10.0.401 (SDK fijado en `global.json`)

**Primary Dependencies**: ASP.NET Core Minimal APIs, Blazor Web App, xUnit, FluentAssertions, bUnit

**Storage**: N/A para esta iniciativa; la persistencia en PostgreSQL y EF Core se reserva para specs posteriores.

**Testing**: xUnit para backend y frontend; FluentAssertions para assertions del backend; bUnit para validación básica de UI en Blazor.

**Target Platform**: Desarrollo local y compilación en Windows/Linux/macOS con .NET 10.

**Project Type**: Aplicación web con backend, frontend y pruebas automatizadas.

**Performance Goals**: Sin objetivos de negocio; la prioridad es una puesta en marcha rápida y una compilación consistente.

**Constraints**: .NET 10 obligatorio; no modificar `global.json`; no controllers; no lógica de negocio; no entidades del dominio; no migraciones ni persistencia de producto.

**Scale/Scope**: Estructura base de la solución; sin casos de uso, sin dominio ni integraciones reales.

## Constitution Check

*GATE: Debe pasar antes de la investigación y volver a comprobarse tras el diseño.*

- ✅ Cumple el principio de solución única y compartida: un único repositorio con backend, frontend y pruebas.
- ✅ Cumple el stack obligatorio: .NET 10, Minimal APIs, Blazor Web App, pruebas unitarias.
- ✅ Respeta la prohibición de controllers y la organización por slices del backend.
- ✅ Respeta la regla de no mezclar dominio con requests/responses o lógica de UI.
- ✅ No introduce funcionalidad de negocio fuera del alcance especificado.
- ✅ No requiere justificación de complejidad adicional: la estructura es directa y compatible con la constitución.

## Project Structure

### Documentation (this feature)

```text
specs/001-solution-foundation/
├── spec.md
├── plan.md
├── research.md
├── data-model.md
├── quickstart.md
├── contracts/
│   └── health-endpoint.md
└── tasks.md
```

### Source Code (repository root)

```text
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
│           └── AppTests.cs
└── global.json
```

**Structure Decision**: Se adopta la estructura exacta definida en la especificación: una solución `Realtor.sln` bajo `app/`, con cuatro proyectos y los nombres indicados para backend, frontend y pruebas. Se evita cualquier estructura alternativa porque la spec es el contrato de esta iniciativa y fija los paths.

## Complexity Tracking

No aplica: no hay violaciones de la constitución que requieran justificación. La solución base es directa y compatible con el stack obligatorio.
