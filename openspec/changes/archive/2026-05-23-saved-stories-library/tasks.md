## 1. Backend - Minimal API Endpoints

- [x] 1.1 Implementar endpoint `GET /api/stories` en `Program.cs` que invoque a `IStoryRepository.GetAllAsync()` y devuelva un listado ordenado descendente de `StorySummary`.
- [x] 1.2 Implementar endpoint `GET /api/stories/{id}` en `Program.cs` que invoque a `IStoryRepository.GetByIdAsync(id)` para cargar los detalles del cuento.
- [x] 1.3 Implementar endpoint `POST /api/stories/{id}/replay` en `Program.cs` que reciba el `connectionId`, cargue el cuento, instancie los canales de streaming y los servicios `TextToAudioService` y `AudioDeliveryService` en scope, y lance las tareas concurrentes para retransmitir las ondas de audio (`audioChunk`) de todo el cuento por SignalR de forma inmediata.

## 2. Frontend - Visual Layout y CSS Animaciones

- [x] 2.1 Agregar las estructuras de contenedores HTML en `index.html` para superponer la cuadrícula de la biblioteca sobre las páginas izquierda y derecha del libro mágico animado (activado con estado `'library'`).
- [x] 2.2 Diseñar y agregar clases de estilo para los medallones en 3D dorado de emojis, el botón flotante de retorno de micrófono (`🎤`), y las flechas medievales en `audio-recorder.module.css`.
- [x] 2.3 Implementar el contenedor central `.central-medallion` con la animación de ola `@keyframes emojiWave` y retrasos escalonados para hacer saltar dinámicamente los 5 emojis en estado de reproducción (`'library-playing'`).
- [x] 2.4 Diseñar y animar el estado vacío de la biblioteca con el libro mágico dur Respirando (`😴 Zzz...`) en CSS.

## 3. Frontend - Lógica de Alpine.js e Integración

- [x] 3.1 Expandir el componente `audioRecorder()` de Alpine.js agregando propiedades de paginación (`libraryStories`, `libraryPage`, `currentPageStories()`), métodos para cargar el historial (`loadLibrary()`), y el marcador de estado `'library'`.
- [x] 3.2 Programar la transición interactiva al hacer clic en un medallón: ocultar la grilla, clonar los emojis en el medallón central, transitar al estado `'library-playing'`, e invocar al backend para iniciar el replay de audio.
- [x] 3.3 Asegurar que las escuchas de SignalR existentes en `index.html` procesen los fragmentos de audio (`audioChunk`) y redirijan automáticamente al estado `'library'` tras recibir la señal de cuento finalizado (`storyComplete`).
- [x] 3.4 Conectar la paginación táctil del libro (hojeado de 8 en 8 cuentos) y la lógica de cierre para volver al portal original de grabación.

## 4. Pruebas y Verificación Funcional

- [x] 4.1 Verificar el estado vacío creando y guardando cuentos nuevos para poblar la biblioteca con medallones.
- [x] 4.2 Probar de punta a punta el flujo infantil: abrir la biblioteca, ojear las páginas, pulsar en un medallón de emojis, comprobar que se bloquea la pantalla, que los emojis bailan onduladamente en el centro mientras suena la voz por SignalR, y que regresa con gracia a su lugar original al finalizar.
