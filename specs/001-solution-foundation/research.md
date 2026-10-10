# Investigación de la fundación de la solución

## Decisiones tomadas

### 1. Base tecnológica del repositorio
- Se mantiene el SDK fijo en `global.json` con `10.0.401` y no se modifica.
- La solución será un único proyecto de solución dotnet que incluye backend, frontend y dos suites de pruebas.

### 2. Organización de la solución
- El punto de entrada de la solución será `app/Realtor.sln`.
- El backend viviría en `app/backend/src/RealtorApi/` y se implementará como ASP.NET Core Minimal APIs sin `Controller`.
- El frontend vivirá en `app/frontend/src/RealtorWeb/` como Blazor Web App con Razor Components.
- Las pruebas seguirán la estructura sugerida por la constitución: una suite de backend y otra de frontend para pruebas unitarias.

### 3. Respuesta de salud
- El primer endpoint público será `GET /health`.
- La respuesta debe ser un JSON simple con estado saludable y marca temporal, sin depender de servicios externos ni de persistencia.

### 4. Estrategia de pruebas
- Para el backend se usará `xUnit` con `FluentAssertions`.
- Para el frontend se usará `xUnit` con `bUnit` para validar componentes base sin introducir lógica de negocio.
- La iniciativa no incorpora tests de integración ni persistencia porque aún no existe dominio ni infraestructura de producto.

## Racional

La decisión principal es crear una solución base que compile sin introducir funcionalidad de negocio. Esto encaja con la constitución del repositorio, que exige un único sistema compartido, stack .NET 10, frontend Blazor y backend Minimal API, y un único punto de entrada para continuar desarrollando las siguientes specs.

La ruta de `app/` es necesaria porque la especificación define explícitamente los proyectos y el nombre de la solución. La decisión de excluir lógica de dominio y entidades desde esta iniciativa es consistente con la restricción de no crear funciones del producto antes de la primera implementación funcional real.

## Alternativas consideradas

### Opción A: Crear una sola solución sin división por app/
- Ventaja: simplifica la estructura inicial.
- Desventaja: contraviene la especificación y la organización requerida por la iniciativa.
- Resultado: descartada.

### Opción B: Usar ASP.NET Core MVC o controllers en el backend
- Ventaja: familiar para algunos equipos.
- Desventaja: la constitución prohíbe controllers y exige Minimal APIs.
- Resultado: descartada.

### Opción C: Incluir lógica de dominio o entidades en esta iniciativa
- Ventaja: podría parecer un avance funcional inmediato.
- Desventaja: viola los requisitos de la especificación y la regla de no implementar negocio en la fundación.
- Resultado: descartada.

### Opción D: Configurar PostgreSQL u otros servicios en la primera fase
- Ventaja: prepara infraestructura más adelante.
- Desventaja: excede el alcance y no es necesario para un endpoint de salud local.
- Resultado: descartada en esta iniciativa.
