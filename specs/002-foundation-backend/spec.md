# Especificación: Fundación interna del backend Realtor

**Iniciativa**: `002-foundation-backend`  
**Fecha**: 2026-10-12  
**Estado**: Implementada  
**Entrada**: Establecer la estructura interna y las capacidades transversales del backend existente, sin implementar funcionalidades de negocio.

## Registro de estado

- **2026-10-12**: La spec pasó de Borrador a Aprobada por decisión de la persona usuaria, registrada inicialmente con el valor `Aprobado`; el valor se normalizó al estado canónico. Al comenzar `speckit.implement`, se realizó la transición permitida de Aprobada a En implementación.
- **2026-10-12**: Transición de En implementación a Implementada tras completar las 21 tareas, verificar que no hay tareas pendientes y registrar la evidencia de compilación y pruebas en `quickstart.md`.

## Contexto

Esta iniciativa continúa `001-solution-foundation`. La solución `app/Realtor.sln` y el proyecto `app/backend/src/RealtorApi/` ya existen y no deben recrearse. La versión de plataforma se deriva del `global.json` existente, que no debe modificarse.

## Escenarios de usuario y pruebas

### Historia 1: Descubrir y mapear endpoints (Prioridad: P1)

Como persona desarrolladora, quiero que los endpoints Minimal API se descubran mediante el contrato `ISlice` y un mecanismo centralizado, para agregar endpoints sin editar `Program.cs` ni mantener registros manuales.

**Por qué esta prioridad**: El descubrimiento centralizado establece la convención común para todos los endpoints futuros y permite migrar la sonda existente sin introducir una feature de producto.

**Prueba independiente**: Una prueba define una implementación de `ISlice` en el proyecto de tests, escanea ese assembly y comprueba que se registra bajo `ISlice` una sola vez. La aplicación mapea los endpoints descubiertos a través del mecanismo central.

**Escenarios de aceptación**:

1. **Dado** un assembly con una implementación concreta de `ISlice`, **cuando** se ejecuta `RegisterSlices`, **entonces** la implementación se registra bajo `ISlice` sin registros duplicados.
2. **Dado** un endpoint nuevo implementado como `ISlice`, **cuando** se inicia la aplicación, **entonces** se descubre y mapea mediante `MapSliceEndpoints` sin agregar una llamada específica a `Program.cs`.
3. **Dado** el conjunto de slices descubiertos, **cuando** se mapean sus rutas, **entonces** el mecanismo no depende de registries manuales ni de nombres de clases.

### Historia 2: Aplicar validación automática a Minimal APIs (Prioridad: P1)

Como persona desarrolladora, quiero que los validators FluentValidation se descubran por assembly y se ejecuten según la convención del endpoint, para obtener errores de entrada uniformes sin registros individuales.

**Por qué esta prioridad**: La validación transversal evita que cada endpoint futuro implemente su propio mecanismo de validación.

**Prueba independiente**: Las pruebas alojan en el proyecto de tests un validator de prueba y un tipo de entrada; verifican que el filtro detecta y ejecuta el validator, que el caso sin validator continúa normalmente y que una validación fallida devuelve HTTP 400 con `ValidationProblemDetails`.

**Escenarios de aceptación**:

1. **Dado** un validator registrado por escaneo de assembly para un tipo de entrada, **cuando** el filtro de validación procesa ese tipo, **entonces** ejecuta el validator.
2. **Dado** un tipo de entrada sin validator, **cuando** el filtro lo procesa, **entonces** continúa la solicitud sin producir un error por ausencia de validator.
3. **Dada** una validación fallida, **cuando** el endpoint procesa la solicitud, **entonces** responde `400 Bad Request` con un payload `ValidationProblemDetails` que incluye los errores de validación.
4. **Dado** un validator de prueba, **cuando** se configura la aplicación de tests, **entonces** puede descubrirse por assembly scanning sin registrarlo individualmente.

### Historia 3: Estandarizar errores esperados y handlers (Prioridad: P2)

Como persona desarrolladora, quiero disponer de contratos compartidos para handlers y errores esperados, para que las futuras slices puedan registrarse automáticamente y expresar errores HTTP de manera consistente.

**Por qué esta prioridad**: Estos contratos preparan capacidades comunes para casos de uso futuros sin implementar ninguno en esta iniciativa.

**Prueba independiente**: Las pruebas verifican el escaneo de handlers basado en `IHandler` y la conversión de un error de `Result` a `ProblemDetails`, incluyendo el código HTTP y el payload esperado.

**Escenarios de aceptación**:

1. **Dado** un handler concreto que implementa `IHandler` en el assembly escaneado, **cuando** se registra la infraestructura, **entonces** se registra automáticamente sin registro individual.
2. **Dado** un resultado fallido cuyo error contiene `code`, `title`, `detail` y `status`, **cuando** se convierte centralmente a `ProblemDetails`, **entonces** la respuesta usa ese status y conserva `title`, `detail` y `code` en el payload.
3. **Dada** la infraestructura transversal, **cuando** un componente futuro necesite logging, **entonces** dispone de `ILogger` para emitir logging estructurado.

### Historia 4: Comprobar la disponibilidad mediante `/health` (Prioridad: P2)

Como persona desarrolladora, quiero que la sonda de infraestructura existente se implemente como `ISlice`, para comprobar el descubrimiento y mapeo centralizado con un endpoint real no relacionado con el negocio.

**Por qué esta prioridad**: Demuestra de extremo a extremo la convención de endpoints conservando el contrato de salud ya aprobado.

**Prueba independiente**: Una prueba HTTP en proceso solicita `GET /health` al host de tests y comprueba el estado HTTP satisfactorio y el payload saludable, validando el descubrimiento y mapeo centralizados.

**Escenarios de aceptación**:

1. **Dado** que la API está iniciada, **cuando** se solicita `GET /health`, **entonces** responde `200 OK` con JSON que incluye `status: "Healthy"`, `service: "RealtorApi"` y `timestamp`.
2. **Dado** que `/health` está implementado como `ISlice`, **cuando** la infraestructura descubre y mapea endpoints, **entonces** la ruta queda disponible sin registro específico en `Program.cs`.
3. **Dada** una solicitud a `/health`, **cuando** se comprueba la respuesta, **entonces** esta no depende de servicios externos, base de datos ni datos de dominio.

## Casos límite

- Un assembly sin implementaciones de `ISlice`, `IHandler` o `IValidator<T>` debe poder escanearse sin provocar errores por ausencia de tipos.
- Un endpoint cuyo tipo de entrada no tenga validator debe continuar mediante pass-through.
- Los errores de validación deben diferenciarse de los errores esperados modelados por `Result`: los primeros usan `ValidationProblemDetails` y HTTP 400; los segundos usan el status y los datos de su error.
- El timestamp de `/health` puede variar entre solicitudes; los campos `status` y `service` y el código HTTP deben cumplir el contrato existente.
- Los scanners deben operar sobre el assembly indicado y no depender de una lista de tipos mantenida manualmente.

## Requisitos

### Requisitos funcionales

- **FR-001**: El backend DEBE conservar la solución y el proyecto creados por `001-solution-foundation`; esta iniciativa NO DEBE recrearlos ni modificar `global.json`.
- **FR-002**: El backend DEBE mantener una organización compatible con Vertical Slice Architecture. La infraestructura compartida DEBE ubicarse bajo `Infrastructure`, separando al menos `Endpoints`, `Validation` y `Handlers`; esta iniciativa NO DEBE crear slices de negocio.
- **FR-003**: El backend DEBE definir el contrato `ISlice` con el método `AddEndpoint(IEndpointRouteBuilder app)` para mapear endpoints Minimal API.
- **FR-004**: La infraestructura DEBE proporcionar `RegisterSlices` para descubrir por reflection y registrar las implementaciones concretas de `ISlice` del assembly indicado bajo el contrato `ISlice`, sin duplicados.
- **FR-005**: La infraestructura DEBE proporcionar `MapSliceEndpoints` como mecanismo centralizado para mapear los slices descubiertos. Añadir un endpoint como `ISlice` NO DEBE requerir modificar `Program.cs`, mantener registries manuales ni descubrir tipos por nombre de clase.
- **FR-006**: Las implementaciones de `ISlice` DEBEN ser stateless y conformarse a las restricciones de ciclo de vida establecidas para endpoints.
- **FR-007**: El backend DEBE definir el marker interface `IHandler` y registrar automáticamente por assembly scanning los handlers concretos que lo implementen, sin registros individuales ni handlers de negocio en esta iniciativa.
- **FR-008**: El backend DEBE registrar validators FluentValidation mediante assembly scanning, sin registros individuales.
- **FR-009**: El backend DEBE proporcionar `ValidationFilterFactory`, que detecte por convención `IValidator<T>` para los tipos de entrada de Minimal API y ejecute la validación cuando exista un validator.
- **FR-010**: Si no existe un validator para el tipo de entrada procesado, el filtro DEBE continuar la solicitud sin error.
- **FR-011**: Si la validación falla, la solicitud DEBE responder con HTTP 400 y `ValidationProblemDetails` que incluya los errores de validación.
- **FR-012**: El backend DEBE ofrecer un modelo `Result` para representar resultados y errores esperados. Cada error esperado DEBE exponer `code`, `title`, `detail` y `status`.
- **FR-013**: El backend DEBE convertir centralmente un error de `Result` en `ProblemDetails`, conservando `title`, `detail` y `status`, y exponiendo `code` como extensión del payload.
- **FR-014**: La infraestructura transversal DEBE permitir logging estructurado mediante `ILogger`; no se requiere introducir eventos de negocio ni logging de casos de uso inexistentes.
- **FR-015**: El endpoint existente `GET /health` DEBE migrarse a una implementación `ISlice` y descubrirse y mapearse por la infraestructura centralizada.
- **FR-016**: `GET /health` DEBE conservar el contrato aprobado en `specs/001-solution-foundation/contracts/health-endpoint.md`: HTTP 200, JSON con `status` igual a `Healthy`, `service` igual a `RealtorApi` y un `timestamp`.
- **FR-017**: `Program.cs` DEBE limitarse a configurar servicios y middleware, registrar la infraestructura transversal y mapear endpoints mediante el mecanismo centralizado, además de iniciar la aplicación.
- **FR-018**: Las clases `ISlice` y validators creados exclusivamente para probar los scanners o la validación DEBEN residir en `app/backend/tests/RealtorApiTests/`, nunca en el proyecto de producción.
- **FR-019**: Las pruebas unitarias DEBEN verificar: descubrimiento y registro único de slices definidos en el assembly de tests; validación presente y pass-through sin validator; respuesta `ValidationProblemDetails` HTTP 400; y conversión de error de `Result` a `ProblemDetails`. Además, DEBE existir una prueba HTTP en proceso para comprobar que `GET /health` responde correctamente después del descubrimiento y mapeo centralizados.
- **FR-020**: La solución y el backend DEBEN compilar con la versión de plataforma derivada del `global.json` existente.
- **FR-021**: Esta iniciativa NO DEBE introducir controllers, entidades de dominio, lógica de negocio, endpoints de producto, handlers de negocio, validators de negocio, mappings de negocio, `AppDbContext`, configuraciones EF Core, migraciones, seeders ni conexiones a base de datos.

### Entidades clave

Esta iniciativa no introduce entidades de dominio ni persistencia. `ISlice`, `IHandler`, `Result`, los errores esperados y `ProblemDetails` son contratos de infraestructura, no entidades de negocio.

## Criterios de éxito

- **SC-001**: La solución y el proyecto backend existentes compilan sin errores con el SDK fijado por `global.json`, sin recrear proyectos ni modificar ese archivo.
- **SC-002**: Una implementación de `ISlice` definida en el proyecto de tests se descubre y registra una sola vez bajo el contrato `ISlice`.
- **SC-003**: Los endpoints Minimal API se mapean mediante `MapSliceEndpoints`; no se requiere modificar `Program.cs` al agregar un slice.
- **SC-004**: Los validators se descubren por assembly; la validación se ejecuta cuando existe `IValidator<T>` y la solicitud pasa sin error cuando no existe.
- **SC-005**: Una validación fallida produce HTTP 400 y un `ValidationProblemDetails` con los errores correspondientes.
- **SC-006**: El mapeo de un error de `Result` conserva el status, título y detalle, y expone el código en el payload `ProblemDetails`.
- **SC-007**: `GET /health` devuelve HTTP 200 y los campos establecidos en su contrato, y está implementado como `ISlice` descubierto centralmente.
- **SC-008**: La revisión del backend confirma que no se añadieron entidades, persistencia, base de datos, endpoints ni casos de uso de negocio.

## Suposiciones

- `001-solution-foundation` es la base existente y su contrato de salud permanece vigente.
- El SDK y el framework objetivo se determinan exclusivamente a partir del `global.json` existente.
- `ProblemDetails` representa errores esperados; `ValidationProblemDetails` representa errores de validación de entrada.
- Los contratos y scanners se diseñan para assemblies explícitamente indicados, permitiendo probar descubrimiento con tipos declarados en el proyecto de tests.
- Las pruebas pertenecen al proyecto `app/backend/tests/RealtorApiTests/`. Se permite únicamente una prueba HTTP en proceso para verificar `/health` a través del pipeline de la aplicación, porque su contrato requiere demostrar el descubrimiento y mapeo de rutas; no se autorizan servicios externos ni dependencias de base de datos.
- El logging estructurado se deja disponible como capacidad transversal; no se inventan eventos ni operaciones de negocio para ejercitarlo.

## Fuera de alcance

- Recrear o reconfigurar la solución y los proyectos de `001-solution-foundation`.
- Crear features, endpoints, handlers, validators, mappings o contratos de negocio.
- Crear entidades de dominio, persistencia, `AppDbContext`, configuraciones EF Core, migraciones, seeders o conexiones a PostgreSQL.
- Incorporar autenticación, autorización, servicios externos o lógica de producto.
- Modificar `global.json` o cambiar la versión de plataforma.