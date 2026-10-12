# Investigación de la fundación interna del backend

## Decisiones

### Plataforma y dependencias

- **Decisión**: Mantener .NET 10 (`net10.0`) según `global.json` y la constitución. Usar Minimal APIs, DI, `ILogger<T>`, `ProblemDetails` y `ValidationProblemDetails` provistos por ASP.NET Core. Incorporar `FluentValidation.DependencyInjectionExtensions` para el registro por assembly.
- **Racional**: La infraestructura solicitada puede construirse con el framework existente y el paquete oficial de integración de DI de FluentValidation. El proyecto backend actual no tiene dependencias de negocio que deban conservarse.
- **Alternativas consideradas**: Controllers, MediatR, una librería externa de resultados, un contenedor DI alternativo o infraestructura de persistencia; se descartan por prohibición explícita o por exceder el alcance.

Las instrucciones backend mencionan .NET 11, mientras la constitución y `global.json` fijan .NET 10. Prevalecen la constitución y la versión ya fijada; no se modifica el SDK.

### Descubrimiento y ciclo de vida

- **Decisión**: `RegisterSlices` y `RegisterHandlers` reciben explícitamente un assembly. Registrar las slices stateless como singleton y las implementaciones concretas de `IHandler` como scoped. No se crean listas de tipos ni dispatchers.
- **Racional**: El mapeo de rutas ocurre al iniciar la aplicación, mientras un handler futuro puede requerir dependencias scoped. Los slices no deben capturar esos handlers en su constructor.
- **Alternativas consideradas**: Descubrimiento global de todos los assemblies cargados, registros manuales o resolución de handlers scoped desde una slice singleton; se descartan por riesgo de duplicados, acoplamiento o ciclo de vida inválido.

### Validación automática

- **Decisión**: Registrar validators con `AddValidatorsFromAssembly` para el assembly indicado y lifetime scoped. Aplicar una `ValidationFilterFactory` al grupo central de endpoints; inspeccionar los parámetros del handler y validar los que tengan un `IValidator<T>` registrado.
- **Racional**: El escaneo oficial de FluentValidation evita un scanner de validators propio. `IServiceProviderIsService` permite comprobar disponibilidad sin fallar cuando no hay validator. `ValidateAsync` propaga el token de cancelación; los errores se agrupan en `ValidationProblemDetails` con HTTP 400.
- **Alternativas consideradas**: Validación manual en cada endpoint, filtros individuales o error por ausencia de validator; se descartan por contradicción con la spec.

### Resultados y errores esperados

- **Decisión**: Definir `Result<T>` y un error compartido con `code`, `title`, `detail` y `status`; centralizar su traducción a `ProblemDetails` y exponer `code` como extensión del payload.
- **Racional**: Cumple el contrato elegido durante `clarify` sin introducir una librería externa ni acoplar el modelo a casos de uso inexistentes.
- **Alternativas consideradas**: Usar excepciones como flujo de errores, respuestas anónimas o una librería de terceros; se descartan porque dificultan la consistencia o amplían innecesariamente las dependencias.

### Pruebas y sonda de salud

- **Decisión**: Mantener pruebas unitarias xUnit para scanners, validación y mapeo de errores. La única prueba HTTP de host será `GET /health` mediante `WebApplicationFactory`, tras la aprobación de la spec y con tarea explícita en `tasks.md`.
- **Racional**: El contrato requiere demostrar que la ruta real pasa por descubrimiento y mapeo centralizados. El test project ya referencia `Microsoft.AspNetCore.Mvc.Testing`; no se requieren servicios externos.
- **Alternativas consideradas**: Usar solo pruebas unitarias para afirmar que el host mapea la ruta o permitir pruebas HTTP para otros endpoints; la primera no comprueba el pipeline y la segunda excede la autorización limitada.

## Límites confirmados

- No se crean entidades, casos de uso, handlers concretos, validators de negocio ni endpoints de producto.
- No se agrega `AppDbContext`, EF Core, Npgsql, PostgreSQL, migraciones, seeders ni conexión a datos.
- La respuesta JSON de `/health` conserva el contrato de `specs/001-solution-foundation/contracts/health-endpoint.md`.
- `ILogger<T>` queda disponible mediante el framework; no se agregan logs ficticios para ejercitarlo.