# Especificación: Catálogo de estados y listado paginado de propiedades

**Iniciativa**: `004-properties-list-pagination`  
**Rama**: `feat/004-properties-list-pagination`  
**Fecha**: `2026-10-15`  
**Estado**: `En implementación`
**Entrada**: Persistir el catálogo de estados de propiedades y exponer un listado paginado con imágenes públicas.

## Contexto

La iniciativa 003 dejó propiedades persistidas con `Status` almacenado como texto derivado del enum `PropertyStatus`, un seeder idempotente basado en manifiesto y activos de imagen publicados bajo `/images/properties`. Esta iniciativa amplía esa base sin modificar ni reabrir la iniciativa 003.

La nueva capacidad reúne dos cambios dependientes: primero, normalizar los estados en un catálogo relacional sin perder propiedades existentes; después, permitir que consumidores consulten propiedades en páginas y reciban enlaces públicos absolutos a sus imágenes.

## Clarifications

### Session 2026-10-15

- Q: ¿Qué clave debe identificar cada estado en `property_status`? → A: `Value` textual como clave primaria.
- Q: ¿Qué límite debe aplicarse a `pageSize`? → A: Máximo 50.
- Q: ¿Qué nombre debe tener la colección de propiedades en la respuesta paginada? → A: `items`.
- Q: ¿Debe `GET /api/properties` permitir consultas anónimas? → A: Sí, acceso público sin autenticación.

## Escenarios de usuario y pruebas

### Historia 1: Mantener estados de propiedad consistentes (Prioridad: P1)

Como responsable de los datos de propiedades, quiero que cada propiedad use un estado existente del catálogo para evitar valores divergentes y conservar la información ya almacenada.

**Prioridad**: El listado y la persistencia futura dependen de que cada estado tenga una representación única y verificable.

**Prueba independiente**: Aplicar la migración a una base nueva y a una base con propiedades existentes; comprobar que los tres estados estén disponibles, que cada propiedad conserve sus datos y que tenga una relación válida con su estado.

**Escenarios de aceptación**:

1. **Dado** una base nueva, **cuando** se aplica la migración y se ejecuta el seeding, **entonces** existe un catálogo con `Available`, `Rented` y `Maintenance`, cada valor una sola vez.
2. **Dada** una base con propiedades cuyos estados son valores admitidos, **cuando** se aplica la migración, **entonces** se conservan todas las propiedades y cada estado queda relacionado con su registro de catálogo.
3. **Dada** una propiedad con un estado que no existe en el catálogo, **cuando** se intenta persistirla, **entonces** la relación obligatoria impide dejar un estado huérfano.
4. **Dada** una base con un estado histórico no reconocido, **cuando** se aplica la migración, **entonces** la operación falla sin reasignar ese estado ni eliminar o alterar la propiedad.

### Historia 2: Sembrar estados y propiedades sin duplicados (Prioridad: P1)

Como responsable de la inicialización de datos, quiero que los estados de cada propiedad se resuelvan desde el catálogo para que una carga repetida mantenga la misma relación y no invente estados alternativos.

**Prioridad**: Las propiedades de seed necesitan estados válidos antes de que el listado pueda ofrecer resultados coherentes.

**Prueba independiente**: Ejecutar dos veces el seeding y comprobar el catálogo, las propiedades y sus referencias de estado después de cada ejecución.

**Escenarios de aceptación**:

1. **Dado** el manifiesto y los archivos de seed vigentes, **cuando** se ejecuta el seeding, **entonces** los estados se leen de `properties-statuses.json` y cada propiedad resuelve su estado por el valor canónico exacto.
2. **Dado** que un estado de una propiedad de seed no existe en el catálogo ni en los valores declarados, **cuando** se carga la propiedad, **entonces** el seeding informa el dato inválido y no lo reemplaza silenciosamente por `Available`.
3. **Dado** que estados y propiedades ya fueron sembrados, **cuando** se repite el seeding, **entonces** no se crean registros duplicados ni se pierde la relación existente.

### Historia 3: Consultar propiedades por páginas con sus imágenes (Prioridad: P1)

Como consumidor del catálogo inmobiliario, quiero consultar propiedades en páginas ordenadas y con enlaces absolutos a sus imágenes para mostrar resultados de forma predecible sin conocer rutas internas del servidor.

**Prioridad**: El listado es el primer flujo de consulta de propiedades y depende del catálogo de estados y de los activos de imagen ya disponibles.

**Prueba independiente**: Consultar el listado sin parámetros, con una segunda página y con parámetros inválidos; verificar contenido, orden, metadatos, estado textual e imágenes usando los valores del host de la petición.

**Escenarios de aceptación**:

1. **Dado** que existen propiedades, **cuando** se consulta `GET /api/properties` sin parámetros, **entonces** se devuelve la primera página de hasta 6 propiedades ordenadas por título ascendente.
2. **Dadas** propiedades suficientes, **cuando** se consulta `GET /api/properties?page=2&pageSize=6`, **entonces** se devuelven los elementos de la segunda página y metadatos coherentes con el total disponible.
3. **Dada** una respuesta con propiedades, **cuando** se serializa cada elemento, **entonces** incluye los campos requeridos y `status` contiene el valor textual canónico del catálogo.
4. **Dada** una propiedad con imagen persistida, **cuando** se devuelve en el listado, **entonces** `imageUrl` es una URL absoluta servible bajo `/assets/properties/{fileName}` construida con el esquema y host de la petición.
5. **Dado** `page` o `pageSize` inválido, **cuando** se consulta el listado, **entonces** la respuesta es HTTP 400 con `ProblemDetails` o `ValidationProblemDetails` y detalle de validación.
6. **Dado** un número de página posterior a la última página, **cuando** se consulta el listado, **entonces** se devuelve una colección vacía con metadatos válidos y sin error de servidor.
7. **Dada** una solicitud anónima válida, **cuando** se consulta `GET /api/properties`, **entonces** el endpoint devuelve el listado sin requerir credenciales.
8. **Dada** una propiedad cuyo valor persistido no contiene un nombre de archivo utilizable, **cuando** se consulta una página que la incluye, **entonces** la API responde HTTP 500 con `ProblemDetails` y no devuelve una página parcial ni una URL inválida.
9. **Dada** una solicitud con un host no permitido, **cuando** se consulta el listado, **entonces** la API responde HTTP 400 con `ProblemDetails` y no refleja ese host en una URL.

## Casos límite

- La base puede estar vacía, contener datos de la iniciativa 003 o tener migraciones pendientes; el cambio debe conservar todas las propiedades persistidas.
- Un valor histórico de estado fuera de `Available`, `Rented` y `Maintenance` no se debe convertir automáticamente a otro valor ni descartar.
- Una ejecución repetida del seeding puede encontrar estados, propiedades e imágenes ya presentes.
- Si no hay propiedades, `items` queda vacío y `totalPages` es `0`; `hasNext` y `hasPrevious` son `false` para la página predeterminada.
- Si `page` supera `totalPages`, se devuelve `items` vacío; `hasNext` es `false` y `hasPrevious` indica si `page` es mayor que `1`.
- Los parámetros ausentes usan sus valores predeterminados; los valores no enteros, cero o negativos son inválidos.
- `pageSize` superior al máximo permitido es inválido; el límite se define en 50 elementos por página.
- Los títulos repetidos conservan un orden de página determinista mediante `id` ascendente como desempate después de `title`.
- Una propiedad sin nombre de archivo de imagen utilizable es una inconsistencia interna; no debe producir una ruta física, una URL relativa ni una respuesta parcial.
- Solo se usan hosts admitidos por la configuración de `AllowedHosts`; un host no permitido no se refleja en `imageUrl`.
- Los encabezados de host o esquema reenviados solo se aceptan desde proxies configurados explícitamente como confiables.

## Requisitos

### Requisitos funcionales

- **FR-001**: El sistema DEBE persistir `PropertyStatus` como entidad de catálogo en la tabla PostgreSQL `property_status`.
- **FR-002**: Cada registro de `property_status` DEBE tener `Value` como clave primaria textual, no vacía y única, y `Description` como descripción requerida. Los valores admitidos en esta iniciativa son exactamente `Available`, `Rented` y `Maintenance`; se conservan con esa capitalización.
- **FR-003**: Cada propiedad DEBE tener un estado obligatorio relacionado con una fila existente de `property_status`. La columna textual `Properties.Status` existente DEBE conservar los valores y pasar a estar protegida por una clave foránea hacia `property_status.Value`; la relación no puede quedar huérfana.
- **FR-004**: La configuración persistente de `Property` y `PropertyStatus` DEBE permanecer en configuraciones separadas del modelo y respetar la convención vigente de persistencia del proyecto.
- **FR-005**: La iniciativa DEBE incorporar una única migración coherente que cree el catálogo, prepare sus registros y aplique la relación sin borrar ni recrear `Properties`.
- **FR-006**: La migración DEBE preservar para cada propiedad existente `Id`, `Title`, `Description`, `Address`, `Price`, estado, datos de habitaciones y área, `ImageUrl`, `CreatedAt` y `UpdatedAt`. Si encuentra un estado sin correspondencia, DEBE fallar de forma determinista antes de una pérdida o reasignación de datos.
- **FR-007**: La reversión de la migración DEBE retirar la relación y el catálogo de esta iniciativa sin eliminar propiedades ni sus valores textuales de estado.
- **FR-008**: El seeding DEBE incorporar idempotentemente los valores y descripciones declarados en `properties-statuses.json`, leído desde el manifiesto existente, y resolver el estado de cada propiedad de seed mediante el catálogo.
- **FR-009**: Un estado desconocido en los datos de seed DEBE producir un error identificable; queda prohibido sustituirlo silenciosamente por `Available`.
- **FR-010**: El seeding repetido DEBE conservar las propiedades existentes y no duplicar estados ni propiedades.
- **FR-011**: El backend DEBE exponer `GET /api/properties` como listado paginado, sin agregar filtros ni parámetros de ordenamiento en esta iniciativa.
- **FR-012**: Los parámetros opcionales `page` y `pageSize` DEBEN usar los valores predeterminados `1` y `6`, respectivamente; deben ser enteros positivos y `pageSize` no puede superar `50`.
- **FR-013**: El listado DEBE ordenar por `title` ascendente y, ante títulos iguales, por `id` ascendente. El conteo total corresponde a todas las propiedades antes de aplicar página y tamaño.
- **FR-014**: La respuesta DEBE usar un objeto con `items` y metadatos `page`, `pageSize`, `totalItems`, `totalPages`, `hasNext` y `hasPrevious`. `totalPages` es `0` cuando `totalItems` es `0`; en otro caso es el redondeo hacia arriba de `totalItems / pageSize`. `hasNext` indica si existen páginas posteriores y `hasPrevious` es verdadero cuando `page > 1`.
- **FR-015**: Cada elemento de `items` DEBE incluir como mínimo `id`, `title`, `description`, `address`, `price`, `status`, `bedroomCount`, `bathroomCount`, `areaSquareMeters` e `imageUrl`. `status` DEBE ser el valor textual exacto de `PropertyStatus.Value`, compatible con el contrato vigente.
- **FR-016**: `imageUrl` DEBE ser una URL absoluta `http` o `https` construida con el esquema y host de la petición y el patrón `/assets/properties/{fileName}`. La ruta pública DEBE servir la imagen correspondiente. No se permiten rutas físicas ni URLs relativas en la respuesta final.
- **FR-017**: Un parámetro ausente usa su valor predeterminado; un valor no numérico, no positivo o un `pageSize` mayor que `50` DEBE producir HTTP 400 con `ProblemDetails` o `ValidationProblemDetails` que identifique el error.
- **FR-018**: La consulta de listado DEBE devolver contratos explícitos, no entidades persistentes; será de solo lectura, proyectará los campos necesarios antes de materializar resultados y no cargará datos de entidades que no formen parte de la respuesta.
- **FR-019**: La ruta HTTP, los validadores, el handler y el mapping del listado DEBEN pertenecer al slice de `Properties/ListProperties`, ubicado bajo `app/backend/src/RealtorApi/Features/`. El slice DEBE seguir `ISlice` y el auto-descubrimiento vigente; no se permiten controllers, registros manuales por endpoint ni cambios de registro individual en `Program.cs`.
- **FR-020**: El caso de uso DEBE propagar `CancellationToken` durante el flujo asíncrono y los parámetros inválidos deben integrarse con el manejo existente de validación y Problem Details.
- **FR-021**: `app/backend/src/RealtorApi/RealtorApi.http` DEBE incluir solicitudes manuales para `GET /api/properties` y `GET /api/properties?page=1&pageSize=6`.
- **FR-022**: `GET /api/properties` DEBE permitir consultas anónimas y no exigir autenticación ni credenciales.
- **FR-023**: La URL absoluta DEBE usar el esquema y host de la solicitud solo después de validar el host contra una lista explícita de `AllowedHosts`; no se DEBE aceptar `*` como política efectiva para esta ruta.
- **FR-024**: Los encabezados reenviados que cambian host o esquema solo DEBEN aceptarse desde proxies configurados explícitamente como confiables. Si el host no está permitido, el endpoint DEBE responder HTTP 400 con `ProblemDetails` y no reflejarlo en la respuesta.
- **FR-025**: Si una propiedad de la página no tiene un nombre de archivo de imagen utilizable, el endpoint DEBE responder HTTP 500 con `ProblemDetails`, sin devolver una página parcial ni una URL relativa, física o inventada.

### Restricciones de arquitectura

- La carpeta de negocio se crea en el root de `RealtorApi` si aún no existe, con la forma `Features/Properties/ListProperties/`; se permiten archivos separados o una implementación equivalente dentro del slice.
- El slice usará la infraestructura transversal ya existente para `ISlice`, registro de handlers y validación; no se crea un scanner paralelo ni se reorganiza infraestructura fuera de este alcance.
- La lectura prioriza una proyección directa de solo lectura y mantiene el endpoint delgado, con la lógica del caso de uso en el handler.
- El mapping que convierte el nombre de archivo persistido a URL absoluta usa los datos de la petición actual, no una base URL fija del entorno.
- Se mantienen las convenciones vigentes de EF Core, PostgreSQL, `IEntityTypeConfiguration<T>`, Minimal APIs, FluentValidation y Problem Details.

### Entidades clave

- **PropertyStatus**: estado válido del catálogo. Su `Value` es la clave textual única y canónica; `Description` explica el estado.
- **Property**: propiedad inmobiliaria existente con sus atributos y una relación obligatoria con `PropertyStatus`. Los demás datos de la propiedad no cambian en esta iniciativa.
- **Elemento de listado**: representación de lectura que contiene los campos públicos requeridos, incluido el valor textual del estado y la URL absoluta de imagen.
- **Página de propiedades**: conjunto ordenado de elementos junto con la página solicitada, tamaño, totales y banderas de navegación.

## Criterios de éxito

- **SC-001**: El 100 % de las propiedades presentes antes de la migración permanece después de aplicarla, con todos sus campos y estado original conservados y una referencia válida al catálogo.
- **SC-002**: Una base nueva termina el seeding con exactamente una entrada por cada uno de los tres valores de estado; repetir el seeding no aumenta el número de estados ni propiedades.
- **SC-003**: Una consulta sin parámetros devuelve hasta 6 resultados en orden ascendente por título; una consulta de página posterior devuelve los elementos y metadatos correspondientes a esa página.
- **SC-004**: Cada respuesta de listado contiene todos los metadatos requeridos y un valor de `status` idéntico al valor canónico del catálogo.
- **SC-005**: El 100 % de los elementos con imagen devuelve una URL absoluta que coincide con el host de la petición y cuya ruta pública sirve el archivo correspondiente; para `http://localhost:5023`, el patrón es `http://localhost:5023/assets/properties/{fileName}`.
- **SC-006**: Todos los casos de parámetros inválidos definidos por esta especificación reciben HTTP 400 con detalle de validación; las páginas fuera del rango disponible no producen error de servidor.
- **SC-007**: Una solicitud a través de un host permitido genera URLs con ese host; un host no permitido recibe HTTP 400 y nunca aparece en `imageUrl`.
- **SC-008**: Una página que contiene una propiedad con un nombre de archivo de imagen inválido recibe HTTP 500 con `ProblemDetails` y no contiene resultados parciales.

## Fuera de alcance

- Modificar los artefactos, código o datos propios de la iniciativa 003.
- Agregar, eliminar o administrar estados mediante operaciones de usuario; esta iniciativa incluye solo los tres estados declarados.
- Agregar filtros de búsqueda, selección de estado, rangos de precio u ordenamientos configurables.
- Agregar endpoints de detalle, creación, actualización o eliminación de propiedades.
- Cambiar el modelo de autenticación, el frontend o los flujos de negocio de alquiler.
- Cambiar la ubicación física de los assets existentes; el alcance es exponer la ruta pública especificada.

## Suposiciones

- La iniciativa 003 está implementada y sus migraciones, datos y archivos de seed son la base vigente.
- Los estados existentes usan uno de los tres valores del enum de la iniciativa 003. Un valor ajeno se considera inconsistencia de datos y no se corrige de forma automática.
- La comparación de `Value` es exacta y conserva la capitalización canónica de los tres estados.
- El máximo de 50 elementos por página limita consultas desproporcionadas; el tamaño predeterminado sigue siendo 6.
- `id` ascendente desempata títulos iguales para que el orden de las páginas sea determinista.
- El nombre del archivo de imagen se obtiene del valor de imagen ya persistido; su origen sigue siendo el manifiesto y los activos de seed existentes.
- La ruta `/assets/properties/{fileName}` queda disponible públicamente en el host de la API y corresponde al archivo asociado a cada propiedad.
- Los esquemas y hosts públicos reflejan la petición validada, incluidos los puertos explícitos. Cada entorno configura sus hosts permitidos y, si usa proxy, sus proxies confiables; no se define un dominio de producción en esta iniciativa.

## Dependencias

- Iniciativa 003 `properties-persistence-seeding`, en estado `Implementada`.
- Manifiesto y archivos actuales de seed de estados, propiedades e imágenes.
- Infraestructura existente de auto-descubrimiento `ISlice`, handlers y validación del backend.

## Registro de estado

- `2026-10-15`: `Aprobada` → `En implementación`; inicio de ejecución de T001-T026 conforme al plan y tareas aprobados.