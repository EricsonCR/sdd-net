# Tasks: Persistencia y seeding inicial de propiedades

**Input**: Design documents from `/specs/003-properties-persistence-seeding/`

**Prerequisites**: plan.md, spec.md, research.md, data-model.md, contracts/seed-manifest.md

**Tests**: Required by the feature specification; tests are included in the relevant story phases.

**Organization**: Tasks are grouped by user story to keep each story independently testable.

## Phase 1: Setup (Shared Infrastructure)

**Purpose**: Confirm the project layout and required data folders before creating the persistence work.

- [X] T001 Validate the existing backend layout and the seed-data folders in `app/backend/src/RealtorApi/` and `support/seed-data/`.
- [X] T002 [P] Confirm the required persistence structure under `app/backend/src/RealtorApi/Domain/Properties/` and `app/backend/src/RealtorApi/Infrastructure/Persistence/`.

---

## Phase 2: Foundational (Blocking Prerequisites)

**Purpose**: Prepare the base domain and persistence contracts that all stories need.

- [X] T003 Create the domain folder structure in `app/backend/src/RealtorApi/Domain/Properties/`.
- [X] T004 [P] Ensure the feature remains compatible with the Vertical Slice organization and does not create parallel endpoint folders.

**Checkpoint**: The domain and persistence structure are ready before user story implementation begins.

---

## Phase 3: User Story 1 - Persistir la base de propiedades y estados (Priority: P1) 🎯 MVP

**Goal**: Define the base property domain and enum values for the persistence model.

**Independent Test**: EF configuration test validates that the enum is stored as string and the required property fields exist in the model.

### Tests for User Story 1

- [X] T005 [P] [US1] Add EF configuration test for `PropertyStatus` and `Property` in `app/backend/tests/RealtorApiTests/Infrastructure/Persistence/PropertyConfigurationTests.cs`.

### Implementation for User Story 1

- [X] T006 [P] [US1] Create `PropertyStatus` enum in `app/backend/src/RealtorApi/Domain/Properties/PropertyStatus.cs`.
- [X] T007 [P] [US1] Create `Property` entity in `app/backend/src/RealtorApi/Domain/Properties/Property.cs`.
- [X] T008 [US1] Validate the property contract against the required fields in `app/backend/src/RealtorApi/Domain/Properties/`.

**Checkpoint**: User Story 1 is modeled and testable without requiring the rest of the persistence work.

---

## Phase 4: User Story 2 - Migración y esquema estable (Priority: P1)

**Goal**: Configure EF Core and generate the single migration that defines the full property-persistence schema.

**Independent Test**: Migration validation confirms the schema includes the property table, status conversion, and expected columns.

### Tests for User Story 2

- [X] T009 [P] [US2] Add migration schema validation test in `app/backend/tests/RealtorApiTests/Infrastructure/Persistence/MigrationSchemaTests.cs`.

### Implementation for User Story 2

- [X] T010 [P] [US2] Create `AppDbContext` with `DbSet<Property>` in `app/backend/src/RealtorApi/Infrastructure/Persistence/AppDbContext.cs`.
- [X] T011 [P] [US2] Create `PropertyConfiguration` in `app/backend/src/RealtorApi/Infrastructure/Persistence/Configurations/PropertyConfiguration.cs`.
- [X] T012 [US2] Ensure `OnModelCreating` only applies configurations by assembly and no inline entity mapping is used in `app/backend/src/RealtorApi/Infrastructure/Persistence/AppDbContext.cs`.
- [X] T013 [US2] Generate the single coherent migration under `app/backend/src/RealtorApi/Infrastructure/Migrations/` and review `Up`, `Down`, and snapshot.

**Checkpoint**: The schema is versioned and the persistence configuration is ready for startup initialization.

---

## Phase 5: User Story 3 - Seeding automático idempotente (Priority: P1)

**Goal**: Run migrations and seed data automatically at startup without duplicate inserts.

**Independent Test**: Seed idempotence and startup async seeding test validate repeated execution without duplicates.

### Tests for User Story 3

- [X] T014 [P] [US3] Add seed idempotence test in `app/backend/tests/RealtorApiTests/Infrastructure/Persistence/DatabaseSeederTests.cs`.
- [X] T015 [P] [US3] Add startup seeding test for `UseAsyncSeeding` in `app/backend/tests/RealtorApiTests/Infrastructure/Persistence/StartupSeedingTests.cs`.

### Implementation for User Story 3

- [X] T016 [US3] Update `app/backend/src/RealtorApi/Program.cs` to run `app.MigrateAsync()` before the host starts.
- [X] T017 [US3] Create `MigrationExtensions` in `app/backend/src/RealtorApi/Infrastructure/Persistence/MigrationExtensions.cs` for startup migration preparation.
- [X] T018 [US3] Implement `DatabaseSeeder` sync and async methods in `app/backend/src/RealtorApi/Infrastructure/Persistence/DatabaseSeeder.cs`.
- [X] T019 [US3] Configure `UseSeeding` and `UseAsyncSeeding` in the EF Core setup and verify no manual fallback seed call is required.

**Checkpoint**: The data layer initializes safely and runs seed logic without duplicating rows.

---

## Phase 6: User Story 4 - Integrar archivos externos y activos de imagen (Priority: P1)

**Goal**: Load seed files and image assets from the central manifest so runtime data is consistent and publishable.

**Independent Test**: Manifest parsing and asset-copy validation verifies the project loads the JSON files and stores the correct image destination.

### Tests for User Story 4

- [X] T020 [P] [US4] Add manifest and JSON loading test in `app/backend/tests/RealtorApiTests/Infrastructure/Persistence/SeedManifestTests.cs`.
- [X] T021 [P] [US4] Add image output validation test in `app/backend/tests/RealtorApiTests/Infrastructure/Persistence/PropertyImageAssetTests.cs`.

### Implementation for User Story 4

- [X] T022 [P] [US4] Add the seed manifest, JSON files, and image assets to the project in `app/backend/src/RealtorApi/RealtorApi.csproj` and `support/seed-data/`.
- [X] T023 [US4] Implement manifest-driven file resolution in `app/backend/src/RealtorApi/Infrastructure/Persistence/DatabaseSeeder.cs` using `support/seed-data/seed-manifest.json`.
- [X] T024 [US4] Copy or synchronize property images into the runtime destination and ensure the final image folder is consumable by the API.

**Checkpoint**: Runtime JSON and image assets are published and available to the backend without hardcoded paths.

---

## Phase 7: User Story 5 - Ruta pública de imagen y compatibilidad funcional (Priority: P2)

**Goal**: Ensure each property points to the public API image path and remains consumable in future property queries.

**Independent Test**: The property image URL resolves to the public path and not to a physical support folder.

### Tests for User Story 5

- [X] T025 [P] [US5] Add public image URL validation test in `app/backend/tests/RealtorApiTests/Infrastructure/Persistence/PropertyImageUrlTests.cs`.

### Implementation for User Story 5

- [X] T026 [US5] Set the final `ImageUrl` value to the runtime public route in `app/backend/src/RealtorApi/Infrastructure/Persistence/DatabaseSeeder.cs`.
- [X] T027 [US5] Validate the image route against the final static asset location and ensure it does not include paths from `support/seed-data`.

**Checkpoint**: Properties expose a stable public image route that can be returned directly by future API queries.

---

## Phase 8: Polish & Cross-Cutting Concerns

**Purpose**: Final verification across the persistence and seeding feature.

- [X] T028 [P] Run a build validation for the solution in `app/Realtor.sln`.
- [X] T029 [P] Run the focused backend test suite in `app/backend/tests/RealtorApiTests/RealtorApiTests.csproj`.
- [X] T030 Validate that the generated migration, seed, and asset behavior match the spec and quickstart requirements in `specs/003-properties-persistence-seeding/quickstart.md`.

---

## Dependencies & Execution Order

### Phase Dependencies

- **Setup (Phase 1)**: No dependencies; validates the base project layout.
- **Foundational (Phase 2)**: Must complete before any story work begins.
- **User Stories (Phases 3-7)**: All depend on the foundational phase; they can proceed in priority order and can be partially parallelized when working on separate files.
- **Polish (Phase 8)**: Depends on all relevant story tasks being complete.

### User Story Dependencies

- **US1 (P1)**: Domain model only; independent from the other stories.
- **US2 (P1)**: Depends on US1 for the property model and configuration contract.
- **US3 (P1)**: Depends on US1 and US2 for migration and schema readiness.
- **US4 (P1)**: Depends on US3 for the seeded runtime path and on the project asset integration.
- **US5 (P2)**: Depends on US4 for route consistency and runtime asset availability.

### Parallel Opportunities

- T005 and T006 can run in parallel within US1 after the model contract is known.
- T010 and T011 can run in parallel within US2 once the domain model exists.
- T014 and T015 can run in parallel within US3.
- T020 and T021 can run in parallel within US4.
- T025 can proceed independently once US4 is complete.

---

## Implementation Strategy

### MVP First

1. Complete Phase 1 and Phase 2.
2. Finish US1 and US2 to establish the persistent model and schema.
3. Validate the migration and aggregate model before entering seed logic.
4. Complete US3 and US4 to ensure runtime startup and asset behavior.
5. Finish US5 and final polish before closing the feature.

### Incremental Delivery

1. Domain model first.
2. Persistence configuration and migration next.
3. Startup seeding and idempotence.
4. Runtime asset pipeline and public image URL.
5. Final verification build and tests.

---

## Notes

- Tasks marked `[P]` are parallelizable when they touch different files and do not depend on incomplete work.
- Every task includes exact file paths to keep implementation traceable.
- Test tasks are written before the implementation tasks they validate in the same story.
- The final validation is a build and a focused backend test run, not a broad repository-wide suite.
