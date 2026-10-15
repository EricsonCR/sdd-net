# Implementation Plan: Persistencia y seeding inicial de propiedades

**Branch**: `003-properties-persistence-seeding` | **Date**: `2026-10-12` | **Spec**: [specs/003-properties-persistence-seeding/spec.md](../spec.md)

**Input**: Feature specification from `/specs/003-properties-persistence-seeding/spec.md`

## Summary

Crear la base de persistencia inicial para propiedades y estados, manteniendo la arquitectura actual del backend Minimal API y la convención de Vertical Slice. La solución incluye una entidad `Property`, un enum `PropertyStatus`, configuración EF Core por `IEntityTypeConfiguration<T>`, una sola migración funcional, y un sistema de seeding idempotente con `UseSeeding` y `UseAsyncSeeding` usando un manifest central de assets.

## Technical Context

**Language/Version**: C# with .NET 10, aligned to the repository `global.json` and the existing backend.

**Primary Dependencies**: ASP.NET Core Minimal APIs, EF Core, Npgsql, PostgreSQL, xUnit and Microsoft.AspNetCore.Mvc.Testing for the focused host tests.

**Storage**: PostgreSQL via Npgsql; schema managed through EF Core migrations and startup seeding.

**Testing**: xUnit for model/configuration checks, migration schema validation, seed idempotence, and runtime asset resolution. No broad integration suite beyond the required focused checks.

**Target Platform**: Backend service running on .NET 10; local development and server deployment.

**Project Type**: Web API backend with vertical-slice organization and persistence support.

**Performance Goals**: correctness and deterministic startup behavior are higher priority than throughput; seed data size is small and fixed.

**Constraints**: no controllers, no inline mapping in `OnModelCreating`, no `HasData`, no business endpoints outside `Features`, no hardcoded seed paths, and only one migration for the coherent functional change.

**Scale/Scope**: initial property model and startup seeding only, laying the groundwork for future property insertion and image retrieval.

## Constitution Check

*GATE: Must pass before Phase 0 research. Re-check after Phase 1 design.*

- ✅ The solution remains a single backend project under the existing repo structure; no parallel architecture or duplicated app is introduced.
- ✅ Vertical Slice Architecture is preserved: the domain and persistence live under `Domain` and `Infrastructure`, while business endpoints remain inside `Features` where they exist.
- ✅ The project stays on .NET 10 and uses the repository’s approved stack: ASP.NET Core Minimal APIs, EF Core, Npgsql and PostgreSQL.
- ✅ No controllers are introduced and no business logic leaks into `DbContext` or UI layers.
- ✅ The design avoids `HasData` and uses `UseSeeding` and `UseAsyncSeeding` during startup as required by the repository database rules.
- ✅ The persistence work is intentionally limited to a single coherent migration and the startup seeding pipeline.
- ✅ The runtime asset strategy resolves image and seed files from a central manifest instead of hardcoded project paths.

## Project Structure

### Documentation (this feature)

```text
specs/003-properties-persistence-seeding/
├── spec.md
├── plan.md
├── research.md
├── data-model.md
├── quickstart.md
├── contracts/
│   └── seed-manifest.md
└── tasks.md
```

### Source Code (repository root)

```text
app/backend/src/RealtorApi/
├── Domain/
│   └── Properties/
│       ├── Property.cs
│       └── PropertyStatus.cs
├── Features/
│   └── Properties/
│       ├── CreateProperty/
│       ├── UpdateProperty/
│       ├── GetPropertyById/
│       ├── ListProperties/
│       └── ChangePropertyStatus/
├── Infrastructure/
│   ├── Persistence/
│   │   ├── AppDbContext.cs
│   │   ├── DatabaseSeeder.cs
│   │   ├── MigrationExtensions.cs
│   │   └── Configurations/
│   │       └── PropertyConfiguration.cs
│   ├── Migrations/
│   └── Files/
├── Program.cs
├── RealtorApi.csproj
├── appsettings.json
└── appsettings.Development.json

support/
└── seed-data/
    ├── seed-manifest.json
    ├── properties.json
    ├── properties-statuses.json
    └── images/
        └── properties/
```

**Structure Decision**: This feature keeps the existing vertical-slice structure and places the persistence model under `Infrastructure/Persistence` and the domain model under `Domain/Properties`, without creating parallel endpoint folders outside of `Features`.

## Complexity Tracking

No constitution violations require exceptions or alternative architecture. The design remains within the approved stack and repository patterns.
