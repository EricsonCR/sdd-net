# Tasks: Fundación de la solución Realtor

**Input**: Design documents from `/specs/001-solution-foundation/`

**Prerequisites**: plan.md, spec.md, research.md, data-model.md, contracts/

## Phase 1: Setup (Shared Infrastructure)

**Purpose**: Preparar la estructura base para la solución y dejarla lista para instalación y compilación.

- [X] T001 Create solution root and app structure in `app/` and `app/Realtor.sln`
- [X] T002 [P] Initialize backend project skeleton in `app/backend/src/RealtorApi/`
- [X] T003 [P] Initialize frontend project skeleton in `app/frontend/src/RealtorWeb/`
- [X] T004 [P] Initialize backend test project in `app/backend/tests/RealtorApiTests/`
- [X] T005 [P] Initialize frontend test project in `app/frontend/test/RealtorWeb/`

---

## Phase 2: Foundational (Blocking Prerequisites)

**Purpose**: Definir la base técnica común antes de abordar la historia de usuario.

**Checkpoint**: La base debe quedar preparada para que la solución y la API puedan compilarse y arrancarse.

- [X] T006 Configure .NET 10 solution and project references for `app/Realtor.sln`
- [X] T007 [P] Add shared build metadata and restore dependencies in `app/` and project files
- [X] T008 [P] Configure backend app host and logging basics in `app/backend/src/RealtorApi/Program.cs`
- [X] T009 [P] Configure frontend app host and styling shell in `app/frontend/src/RealtorWeb/Program.cs` and `app/frontend/src/RealtorWeb/wwwroot/app.css`
- [X] T010 Confirm health contract alignment with `specs/001-solution-foundation/contracts/health-endpoint.md`

---

## Phase 3: User Story 1 - Preparar una solución compilable (Priority: P1) 🎯 MVP

**Goal**: Dejar la solución principal compilable con los cuatro proyectos requeridos y el SDK fijado por `global.json`.

**Independent Test**: Compilar `app/Realtor.sln` con el SDK 10.0.401 y comprobar que los cuatro proyectos existen en las rutas requeridas.

### Tests for User Story 1

- [X] T011 [P] [US1] Create solution build smoke test in `app/backend/tests/RealtorApiTests/`
- [X] T012 [P] [US1] Create frontend smoke project setup in `app/frontend/test/RealtorWeb/`

### Implementation for User Story 1

- [X] T013 [US1] Add backend project file and default app config in `app/backend/src/RealtorApi/`
- [X] T014 [US1] Add frontend project file and root component in `app/frontend/src/RealtorWeb/`
- [X] T015 [US1] Add `app/Realtor.sln` with the four required project entries and solution-level references
- [X] T016 [US1] Validate project paths and compile the full solution using the SDK defined in `global.json`

**Checkpoint**: La solución debe quedar compilable sin introducir lógica de negocio ni entidades de dominio.

---

## Phase 4: User Story 2 - Comprobar que la API está disponible (Priority: P2)

**Goal**: Dejar el backend arrancado y responder correctamente a `GET /health` con un estado saludable.

**Independent Test**: Iniciar la API y ejecutar una petición HTTP a `GET /health` para comprobar HTTP 200 y respuesta saludable.

### Tests for User Story 2

- [X] T017 [P] [US2] Add backend endpoint test for `/health` in `app/backend/tests/RealtorApiTests/HealthEndpointTests.cs`

### Implementation for User Story 2

- [X] T018 [US2] Implement minimal health route in `app/backend/src/RealtorApi/Program.cs`
- [X] T019 [US2] Return healthy status payload with a timestamp and service name in `app/backend/src/RealtorApi/Program.cs`
- [X] T020 [US2] Run startup smoke check and verify `GET /health` returns HTTP success in the local API process

**Checkpoint**: El backend debe responder bien al endpoint de salud sin depender de negocio ni persistencia.

---

## Phase 5: Polish & Cross-Cutting Concerns

**Purpose**: Revisar la integridad global de la base antes de cerrar la iniciativa.

- [X] T021 [P] Review repository compliance with the constitution and spec in `specs/001-solution-foundation/spec.md`
- [X] T022 [P] Verify no business logic, entities, or product features were introduced in `app/backend/src/RealtorApi/` and `app/frontend/src/RealtorWeb/`
- [X] T023 Run final validation following `specs/001-solution-foundation/quickstart.md`
- [X] T024 Final review of the solution build and `GET /health` status using the app and backend paths defined in the feature plan

---

## Dependencies & Execution Order

### Phase Dependencies

- **Phase 1 (Setup)**: No dependencies; puede arrancar de inmediato.
- **Phase 2 (Foundational)**: Depende del setup; bloquea todas las historias de usuario.
- **Phase 3 (User Story 1)**: Depende del foundational; es la base del MVP.
- **Phase 4 (User Story 2)**: Depende del foundational y puede ejecutarse en paralelo con la preparación de la solución una vez que la base esté estable.
- **Phase 5 (Polish)**: Depende de que la solución y la API estén validadas.

### User Story Dependencies

- **US1**: Preparar la solución compilable; no depende de otras historias de usuario.
- **US2**: Comprobar disponibilidad del backend; depende de la base y del primer arranque correcto.

### Parallel Opportunities

- Los proyectos de setup pueden crearse en paralelo.
- Las tareas de configuración del backend y del frontend son paralelizables dentro del foundational.
- Las pruebas de la historia 1 y la historia 2 pueden prepararse en paralelo una vez que la base esté lista.

---

## Parallel Example: Story Execution

```bash
# Setup parallel
Task: "Initialize backend project skeleton in app/backend/src/RealtorApi/"
Task: "Initialize frontend project skeleton in app/frontend/src/RealtorWeb/"
Task: "Initialize backend test project in app/backend/tests/RealtorApiTests/"
Task: "Initialize frontend test project in app/frontend/test/RealtorWeb/"

# Story 1 validation
Task: "Compile app/Realtor.sln using SDK 10.0.401"
Task: "Check project paths in app/backend and app/frontend"
```

---

## Implementation Strategy

### MVP First

1. Completar Phase 1.
2. Completar Phase 2.
3. Completar Phase 3 (User Story 1).
4. Validar compilación completa.
5. Avanzar a Phase 4 si el MVP queda estable.

### Incremental Delivery

1. Base técnica y solución compilable.
2. Arranque de la API y endpoint `/health`.
3. Revisión final para evitar lógica de negocio y cumplir la especificación.

### Notes

- [P] indica tareas que pueden ejecutarse en paralelo con archivos distintos.
- Cada tarea incluye un path preciso para responder al requisito de trazabilidad.
- Las tareas están orientadas a ser ejecutables sin más contexto.
- La historia 1 es MVP y la historia 2 añade la validación de arranque del backend.
