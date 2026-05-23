## Why

Actualmente, StoryForge permite generar cuentos de manera interactiva a través de voz, pero carece de una interfaz para navegar y volver a escuchar los cuentos que han sido guardados. Dado que el público objetivo principal es una niña de 4 años que no sabe leer, necesitamos una "Biblioteca Mágica de Cuentos" completamente visual, interactiva y lúdica. La biblioteca presentará cada cuento guardado como un medallón gema interactiva decorada con sus 5 emojis característicos. Esto permitirá a la niña navegar de forma autónoma por su historial de cuentos, escuchar sus historias favoritas y disfrutar de una experiencia reactiva libre de distracciones de texto.

## What Changes

- **Biblioteca Mágica de Dos Páginas**: Una vista alternativa del grimorio que se activa pulsando un botón flotante en forma de marcador de página dorado o llave brillante. Esta vista ocupará ambas páginas del libro interactivo con una rejilla limpia de cuentos.
- **Medallones de Emojis como Reliquias**: Cada cuento guardado se representará visualmente como un medallón elíptico en 3D que contiene sus 5 emojis característicos (ej. `🦁👑✨🌅🌲`). No se mostrarán textos ni párrafos.
- **Reproducción Enfocada e Interactiva**: Al pulsar un medallón, el resto se desvanecerá suavemente y se bloqueará la interacción. El medallón elegido se moverá mágicamente al centro del libro, aumentará de tamaño y sus emojis saltarán/ondularán en bucle para denotar que el cuento está sonando en voz alta, acompañados de notas musicales y estrellas flotantes.
- **Paginación Infantil**: Botones de flechas medievales grandes (`◀` y `▶`) para "hojear" el grimorio físico con una transición de giro de página de 8 en 8 medallones.
- **Botón de Grabación Destacado**: Un gran botón brillante de micrófono (`🎤`) en el lomo del libro para regresar instantáneamente a la pantalla principal de invención de cuentos.
- **Endpoints de Historial y Replay en el Backend**:
  - `GET /api/stories`: Devuelve los IDs y los 5 emojis de todos los cuentos guardados para poblar la biblioteca.
  - `GET /api/stories/{id}`: Devuelve los detalles de un cuento (para uso interno o soporte futuro).
  - `POST /api/stories/{id}/replay?connectionId={id}`: Reactiva de forma dinámica el flujo concurrente de Text-To-Audio (TTS) y AudioDelivery a través del SignalR existente para transmitir el audio por trozos al cliente en tiempo real.

## Capabilities

### New Capabilities

- `saved-stories-library`: Biblioteca mágica de cuentos guardados pensada para niños de corta edad, que permite navegar mediante gemas con 5 emojis y escuchar las narraciones por streaming dinámico sobre SignalR, bloqueando otras acciones y animando los emojis en reproducción.

### Modified Capabilities

*(Ninguna. Las capacidades existentes de repositorio y modelo se mantienen intactas; solo exponemos endpoints nuevos y consumimos la base de datos sqlite sin alterar sus esquemas).*

## Impact

- **Frontend (`wwwroot/index.html` y `audio-recorder.module.css`)**:
  - Incorporar la vista de biblioteca, los nuevos estados en Alpine.js (`library` y `library-playing`), y las clases CSS para el grid, las gemas, las flechas y la animación de baile de emojis.
- **Backend (`Program.cs`, `Hubs`, `Services`)**:
  - Añadir en `Program.cs` los endpoints de lectura y de reproducción dinámica reutilizando los canales de SignalR existentes y los servicios DI en scope.
