# Especificación: Fundación de la solución Realtor

**Iniciativa**: `001-solution-foundation`  
**Fecha**: 2026-10-10  
**Estado**: Borrador  
**Entrada**: Crear la estructura inicial de la solución Realtor, sin lógica de negocio ni funcionalidades de producto.

## Escenarios de usuario y pruebas

### Historia 1: Preparar una solución compilable (Prioridad: P1)

Como persona desarrolladora, quiero disponer de una solución única con proyectos base de backend, frontend y sus respectivas pruebas, para poder continuar las iniciativas de Realtor sobre una estructura común.

**Prioridad**: Es el fundamento para todo trabajo posterior de la aplicación.

**Prueba independiente**: Verificar las rutas y referencias de los cuatro proyectos y compilar la solución completa usando el SDK determinado por `global.json`.

**Escenarios de aceptación**:

1. **Dado** un entorno con el SDK indicado por `global.json`, **cuando** se compila `app/Realtor.sln`, **entonces** los cuatro proyectos incluidos compilan sin errores.
2. **Dado** el repositorio, **cuando** se inspecciona la solución, **entonces** contiene el backend, el frontend y ambos proyectos de pruebas en las rutas definidas en esta especificación.

### Historia 2: Comprobar que la API está disponible (Prioridad: P2)

Como persona desarrolladora, quiero consultar un endpoint de salud básico para comprobar que la API inicia y responde, sin depender de funcionalidades de negocio.

**Prioridad**: Permite comprobar el arranque del backend antes de añadir casos de uso.

**Prueba independiente**: Iniciar la API y solicitar `GET /health`; la respuesta debe indicar un estado saludable y usar un código HTTP de éxito.

**Escenarios de aceptación**:

1. **Dado** que la API está iniciada, **cuando** se solicita `GET /health`, **entonces** se recibe un código HTTP de éxito y una respuesta que indica que el servicio está saludable.
2. **Dado** que la API no está disponible, **cuando** se intenta solicitar `GET /health`, **entonces** la solicitud no se considera satisfactoria.

## Casos límite

- Si el SDK indicado por `global.json` no está instalado, la iniciativa no cambia `global.json`; la compilación queda bloqueada hasta disponer de ese SDK.
- Si los paquetes necesarios no pueden restaurarse, la solución no se considera compilada correctamente.
- Las rutas o proyectos adicionales no deben introducir lógica de negocio, entidades de dominio ni funcionalidades de producto.

## Requisitos

### Requisitos funcionales

- **FR-001**: La solución principal DEBE crearse en `app/Realtor.sln` e incluir los cuatro proyectos definidos en esta especificación.
- **FR-002**: El backend DEBE crearse en `app/backend/src/RealtorApi/` como una aplicación ASP.NET Core basada en Minimal APIs. No DEBE contener controllers.
- **FR-003**: El frontend DEBE crearse en `app/frontend/src/RealtorWeb/` como una Blazor Web App con Razor Components.
- **FR-004**: El proyecto de pruebas del backend DEBE crearse en `app/backend/tests/RealtorApiTests/`.
- **FR-005**: El proyecto de pruebas del frontend DEBE crearse en `app/frontend/test/RealtorWeb/`.
- **FR-006**: Los proyectos DEBEN usar .NET 10, de acuerdo con la versión del SDK fijada en `global.json` (`10.0.401`). El valor de destino de .NET DEBE corresponder a esa versión.
- **FR-007**: `Program.cs` DEBE limitarse a configuración base de servicios, middleware y mapeo de endpoints. No DEBE contener lógica de negocio ni orquestación de casos de uso.
- **FR-008**: El backend DEBE mapear `GET /health` para devolver un estado saludable con un código HTTP de éxito.
- **FR-009**: Esta iniciativa NO DEBE implementar lógica de negocio, funcionalidades de producto ni entidades de dominio.
- **FR-010**: Esta iniciativa NO DEBE crear ni modificar `global.json`.
- **FR-011**: La solución y los cuatro proyectos DEBEN compilar sin errores con el SDK fijado en `global.json`.

## Criterios de éxito

- **SC-001**: La solución incluye el 100 % de los cuatro proyectos requeridos en sus rutas especificadas.
- **SC-002**: Una compilación completa de `app/Realtor.sln` termina sin errores usando el SDK fijado en `global.json`.
- **SC-003**: `GET /health` responde con un código HTTP de éxito y comunica un estado saludable.
- **SC-004**: La revisión de `Program.cs` confirma que contiene únicamente configuración base, sin lógica de negocio.
- **SC-005**: La revisión de la solución confirma que no se añadieron entidades de dominio ni funcionalidades de producto.

## Suposiciones

- El entorno de desarrollo tendrá instalado el SDK seleccionado por `global.json`, actualmente `10.0.401`.
- El framework objetivo para los proyectos será .NET 10 (`net10.0`).
- Los proyectos de pruebas se preparan para pruebas unitarias, conforme a la constitución; la selección de framework de pruebas se resolverá en el plan.
- La respuesta de `GET /health` será local y básica, sin comprobar dependencias externas ni persistencia.
- Los nombres y rutas indicados son el contrato de esta iniciativa; la organización interna detallada se definirá en el plan.

## Fuera de alcance

- Casos de uso, reglas de negocio, entidades de dominio o persistencia.
- Autenticación, autorización e integración con servicios externos.
- Funcionalidades de Realtor, pantallas de producto o contratos de negocio.
- Modificación del SDK fijado en `global.json`.
