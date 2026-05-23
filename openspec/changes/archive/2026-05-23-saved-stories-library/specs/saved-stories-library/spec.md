## ADDED Requirements

### Requirement: Biblioteca Visual y Paginación Infantil
El sistema SHALL mostrar una interfaz de biblioteca integrada en el libro que sea completamente visual, interactiva y libre de texto, adecuada para niños de corta edad.

#### Scenario: Visualización del grimorio vacío
- **WHEN** el usuario abre la biblioteca y no hay cuentos guardados en la base de datos
- **THEN** la biblioteca SHALL mostrar en el centro de las páginas un libro durmiendo (`😴📖`) con una animación de respiración lenta (pulso) y letras `Zzz...` flotando en bucle.

#### Scenario: Visualización de medallones en cuadrícula
- **WHEN** hay cuentos guardados en la base de datos
- **THEN** el sistema SHALL renderizar cada cuento como un medallón elíptico en 3D dorado que contiene únicamente sus 5 emojis característicos, distribuyendo hasta 8 medallones por vista abierta (4 en la página izquierda y 4 en la derecha).

#### Scenario: Navegación de páginas mediante flechas
- **WHEN** el usuario pulsa las flechas gigantes de navegación (`◀` o `▶`) en los márgenes de las páginas
- **THEN** la biblioteca SHALL "hojear" el libro, mostrando el siguiente o anterior bloque de 8 cuentos aplicando una animación CSS de giro de página física.

#### Scenario: Regresar al portal de grabación
- **WHEN** el usuario pulsa el gran botón de micrófono brillante (`🎤`) situado en el lomo del libro
- **THEN** la biblioteca SHALL cerrarse con un efecto de destello de estrellas y restaurar el portal central de grabación en estado inactivo.

### Requirement: Flujo de Reproducción Enfocada de Medallones
El sistema SHALL bloquear cualquier interacción durante la reproducción y animar el medallón seleccionado en el centro del libro para denotar la actividad del cuento.

#### Scenario: Selección y animación de reproducción
- **WHEN** la niña toca un medallón de emojis de la cuadrícula
- **THEN** el sistema SHALL desvanecer el resto de medallones y botones (`opacity: 0.05`, `scale: 0.9`), SHALL mover el medallón seleccionado al centro exacto del libro aumentando su tamaño, y SHALL hacer que sus 5 emojis ondulen y salten en bucle tipo ola mientras se reproducen las notas musicales (`🎵`) y estrellas flotantes.

#### Scenario: Bloqueo total de interacción
- **WHEN** un cuento se está reproduciendo en el centro de la biblioteca
- **THEN** el sistema SHALL inhabilitar cualquier interacción táctil, clic o pulsación sobre la pantalla para evitar interrupciones o cambios de página accidentales por parte de la niña.

#### Scenario: Fin de reproducción y restauración
- **WHEN** el backend emite la señal de cuento finalizado (`storyComplete`)
- **THEN** el medallón central SHALL encogerse y deslizarse de vuelta a su ranura original en la cuadrícula, y todos los demás medallones y controles de la biblioteca SHALL restaurar su opacidad y estado activo.

### Requirement: API de Historial y Streaming de Voz Dinámica
El backend de la aplicación SHALL proporcionar endpoints HTTP para listar la biblioteca y para retransmitir la narración de voz de un cuento a través de la conexión activa de SignalR.

#### Scenario: Listado ligero de cuentos
- **WHEN** el cliente realiza una petición `GET /api/stories`
- **THEN** el servidor SHALL retornar un JSON array con objetos de tipo `StorySummary` que contengan únicamente el `Id` (GUID) y la insignia `Badge` (string de 5 emojis), ordenados por fecha de creación descendente.

#### Scenario: Carga completa de detalles
- **WHEN** el cliente realiza una petición `GET /api/stories/{id}`
- **THEN** el servidor SHALL retornar el objeto `Story` completo con su `Id`, `Title`, `Badge`, `Paragraphs` y `CreatedAt`.

#### Scenario: Retransmisión de audio por SignalR
- **WHEN** el cliente realiza una petición `POST /api/stories/{id}/replay?connectionId={connectionId}`
- **THEN** el servidor SHALL cargar el cuento de la base de datos sqlite, SHALL instanciar canales de audio temporales en scope, y SHALL poner a los servicios `TextToAudioService` y `AudioDeliveryService` a generar y transmitir en segundo plano los fragmentos de audio (`audioChunk`) de todo el cuento (título + párrafos) al cliente en tiempo real.
