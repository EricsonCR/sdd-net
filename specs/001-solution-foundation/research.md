# Investigación de la fundación de la solución

## Decisiones

### 1. Base tecnológica del repositorio
- **Decisión**: Usar .NET 10 con el SDK `10.0.401` fijado en `global.json` en la raíz; no modificar ese archivo.
- **Racional**: La constitución y la spec 001 establecen .NET 10 y los proyectos existentes apuntan a `net10.0`.
- **Alternativas consideradas**: .NET 11 aparece en las instrucciones locales de backend, pero contradice la constitución; se descarta para esta iniciativa porque la constitución prevalece.

### 2. Organización de la solución
- **Decisión**: Conservar `app/Realtor.sln` y los cuatro proyectos existentes en las rutas definidas por la spec. El backend usa Minimal APIs y el frontend Blazor Web App con Razor Components.
- **Racional**: Las rutas son el contrato aprobado de la iniciativa y mantienen una única solución compartida sin controllers.
- **Alternativas consideradas**: Crear otra solución, mover proyectos o usar controllers; se descartan porque contradicen la spec o la constitución.

### 3. Contrato de salud y verificación
- **Decisión**: Mantener `GET /health` con HTTP 200 y JSON que incluya `status`, `service` y `timestamp`, conforme a `contracts/health-endpoint.md`.
- **Racional**: La sonda comprueba el arranque de la API sin depender de lógica de producto, persistencia ni servicios externos.
- **Alternativas consideradas**: Consultar bases de datos u otros servicios desde la sonda; se descarta porque excede el alcance y haría que la disponibilidad básica dependiera de infraestructura externa.

### 4. Estrategia de pruebas
- **Decisión**: Usar xUnit en los dos proyectos de pruebas. Mantener las pruebas unitarias y permitir únicamente una prueba HTTP en proceso con `WebApplicationFactory` para `GET /health`.
- **Racional**: La prueba de host verifica el contrato HTTP y el arranque real de la ruta solicitada. Los proyectos existentes usan xUnit; el backend ya referencia `Microsoft.AspNetCore.Mvc.Testing`.
- **Límite constitucional**: La excepción no habilita otras pruebas de integración. Conforme a la constitución, solo puede usarse cuando la spec esté Aprobada y la justificación esté explícita también en plan y tareas; T017 debe nombrar expresamente la prueba antes de habilitar una nueva implementación.
- **Alternativas consideradas**: Usar únicamente pruebas unitarias no verificaría la ruta a través del host. Usar `WebApplicationFactory` para toda la suite excedería la excepción mínima necesaria. No se agregan FluentAssertions ni bUnit porque no están en los proyectos existentes ni son requeridos por la spec.

## Síntesis

La solución mantiene las rutas, plataformas y límites ya aprobados, y no introduce dependencias de producto. La prueba de host de `/health` queda acotada y condicionada a las transiciones y la trazabilidad exigidas por la constitución.
