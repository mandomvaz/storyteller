## Context

StoryForge es un narrador interactivo con backend en .NET 10 y un frontend de página única servido estáticamente desde `wwwroot/` utilizando Alpine.js y SignalR. Actualmente, los cuentos generados se pueden guardar en una base de datos SQLite local, pero la interfaz no permite navegar ni volver a escucharlos. Como el usuario final es una niña de 4 años, diseñaremos una biblioteca completamente visual, interactiva y robusta ante clics accidentales.

## Goals / Non-Goals

**Goals:**
- Crear una interfaz de biblioteca gema e inmersiva de doble página dentro del grimorio existente.
- Presentar los cuentos como medallones interactivos basados únicamente en sus 5 emojis característicos.
- Bloquear la pantalla y animar el medallón seleccionado en el centro con un baile de emojis dinámico durante la reproducción.
- Implementar paginado táctil y la opción de volver a la grabación fácilmente.
- Reutilizar el pipeline de SignalR existente del backend para transmitir por streaming la voz del cuento de forma inmediata al cliente.

**Non-Goals:**
- Mostrar cualquier tipo de texto o transcripción del cuento en la biblioteca.
- Modificar los modelos existentes (`Story`) o cambiar el esquema de la tabla de SQLite.
- Pre-renderizar archivos de audio estáticos en disco (el almacenamiento y ancho de banda se optimizan haciendo streaming dinámico en scope).

## Decisions

### 1. Gestión de Estados en Alpine.js
Añadiremos dos nuevos estados al componente del frontend:
*   `'library'`: Estado de navegación por la cuadrícula de cuentos guardados.
*   `'library-playing'`: Estado de bloqueo de pantalla y reproducción del cuento activo.
*   *Alternativas*: Crear un segundo componente Alpine.js independiente.
*   *Decisión*: Integrarlo en el mismo componente `audioRecorder()` para compartir fácilmente la instancia de SignalR (`connection`), el manejador de reproducción de audio (`playbackQueue`, `playNext`), y el control de sparkles/celebración.

### 2. Animación de "Medallón Central"
Para llevar el medallón seleccionado al centro:
*   Utilizaremos un contenedor HTML centralizado y dedicado (`.central-medallion`) que se activa únicamente en el estado `'library-playing'`.
*   *Alternativas*: Animar físicamente el elemento del grid mediante cálculos de coordenadas dinámicas (FLIP).
*   *Decisión*: El contenedor dedicado es mucho más robusto para responsive design y multidispositivo. Al hacer clic en un medallón del grid, este se oculta en el grid y se clona su contenido en el contenedor central, el cual se escala y comienza la animación ondulada de emojis.

### 3. Reutilización del Pipeline TTS por SignalR para Replay
Cuando el usuario solicita reproducir un cuento histórico:
*   Invocamos un POST a `/api/stories/{id}/replay?connectionId={connectionId}`.
*   El backend resuelve la base de datos, obtiene el cuento, crea un canal en scope y ejecuta concurrentemente las tareas de `TextToAudioService.RunAsync()` y `AudioDeliveryService.RunAsync()`.
*   *Alternativas*: Generar un único archivo `.wav` estático y retornarlo por HTTP.
*   *Decisión*: El streaming de SignalR es instantáneo (tarda 1-2 segundos en empezar a sonar la primera frase). Compilar un archivo entero de audio de XTTS tardaría de 10 a 15 segundos de carga frustrantes para una niña.

### 4. Animación del Baile de Emojis ("Wave Animation")
Para indicar visualmente la reproducción activa, crearemos una animación CSS escalonada en los 5 emojis del medallón central:
*   Usaremos `@keyframes emojiWave { 0%, 100% { transform: translateY(0); } 50% { transform: translateY(-15px) scale(1.15); } }`
*   Aplicaremos un `animation-delay` incremental a cada uno de los 5 elementos de la insignia (ej. `0s`, `0.15s`, `0.3s`, `0.45s`, `0.6s`) logrando un efecto de ola física y fluida sumamente divertido.

## Risks / Trade-offs

- **[Riesgo] Clics repetitivos o accidentales** → **[Mitigación]** El estado `'library-playing'` bloquea completamente los punteros de pantalla (`pointer-events: none` en la UI superior y `disabled` en botones), garantizando que no se puedan enviar peticiones duplicadas ni interrumpir la reproducción en curso.
- **[Riesgo] Cuentos sin badge/emojis generados en la base de datos** → **[Mitigación]** En el backend, si una historia guardada no tiene emojis o su badge está vacío, se le asigna un fallback por defecto (`📖✨🔮🧙‍♂️🏰`) para que el medallón siempre sea visible y se pueda pulsar.
