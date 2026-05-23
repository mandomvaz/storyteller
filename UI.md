# Guía de la Interfaz de Usuario (UI) de StoryForge 🎨✨

Este documento detalla la arquitectura, el diseño estético, las piezas clave y el funcionamiento de la capa de interfaz de usuario de **StoryForge**.

---

## 🏛️ Filosofía de Arquitectura de la UI

La interfaz de StoryForge se diseñó bajo una premisa fundamental: **máximo rendimiento visual con cero sobrecarga de compilación (Zero-Build Frontend)**. 

En lugar de utilizar frameworks pesados que requieren transpilación (como React, Angular o Vue con Webpack o Vite), la UI se ha estructurado utilizando **HTML5 semántico**, **CSS nativo avanzado** (módulos y variables CSS) y **Alpine.js** cargado vía CDN. Esto permite:
1. **Cero dependencias** en el frontend durante el desarrollo y despliegue.
2. **Interactividad reactiva inmediata** y de bajísima latencia.
3. **Carga ultrarrápida** ideal para dispositivos móviles o de bajo rendimiento.

---

## 📁 Archivos Implicados en la UI

La interfaz se divide limpiamente en dos aplicaciones independientes de una sola página (SPA) que residen en la carpeta `wwwroot/`:

```
storyforge/wwwroot/
│
├── index.html                   # SPA de la Aplicación Infantil (Grabadora y Reproductor)
├── audio-recorder.module.css    # Estilos CSS, animaciones y VFX de la App Infantil
│
└── backoffice/
    ├── index.html               # SPA de Administración (Consola del Cuentacuentos)
    └── backoffice.css           # Estilos oscuros, grid modular y diseño responsive del Backoffice
```

---

## 🧒 1. Aplicación Infantil Principal (`index.html`)

Es la pantalla principal orientada al usuario final (los niños). Está tematizada como un **libro mágico interactivo** de cuentos. Su interactividad está orquestada por una máquina de estados reactiva en Alpine.js.

### 🔄 La Máquina de Estados del Cliente
El comportamiento completo de la interfaz está determinado por la variable reactiva `state` en Alpine.js:

| Estado (`state`) | Elementos Visibles | Comportamiento en Segundo Plano |
|---|---|---|
| `warming` | Portal celestial animado de precalentamiento. | Realiza un `POST /api/stories/warmup` y conecta SignalR. |
| `idle` | Botón central de micrófono brillante. | En reposo, listo para iniciar una grabación. |
| `recording` | Botón central de parada (Stop cuadrado). | Captura audio del micrófono usando la API `MediaRecorder`. |
| `loading` / `transcribing` / `writing` | Estrella mágica giratoria de 5 puntas. | Whisper transcribe el audio u Ollama genera el texto en streaming. |
| `playing` | Botón de nota musical. Notas volando y libro brillando. | Reproduce en cola secuencial los fragmentos de audio recibidos por SignalR. |
| `deciding` | Botones de Guardar (Corazón) y Descartar (Basura). | Espera la decisión del usuario para persistir el cuento recién creado. |
| `library` | Libro abierto en modo biblioteca con medallones. | Carga la lista de cuentos guardados desde la API `/api/stories`. |
| `library-playing` | Medallón central gigante con emojis ondeando. | SignalR transmite el audio sintetizado de una historia histórica seleccionada. |
| `error` | Nube de error azul / Carita triste rebotando. | Avisa visualmente si falla el micrófono o la conexión. Vuelve a `idle` a los 5s. |

---

### 🎨 Efectos Visuales Magic-VFX (CSS & SVG)
La experiencia mágica se construye con técnicas avanzadas de animación CSS nativa combinada con gráficos vectoriales:

1. **Portal Celestial de Calentamiento (`state === 'warming'`)**:
   Un SVG del orbe mágico con degradados radiales, filtros de resplandor (`feGaussianBlur`) y dos círculos concéntricos de estrellas rotando en direcciones opuestas (`animation: rotateGoldRing`). Además, genera partículas flotantes doradas en la pantalla usando un array reactivo (`warmupSparkles`).
2. **Estrellas y Brillos de Fondo (`bg-sparkles`)**:
   Un sistema de partículas dinámico. Al inicializar Alpine.js, se puebla una lista aleatoria de 25 brillos con posiciones, tamaños y retrasos de animación aleatorios que parpadean suavemente de fondo.
3. **Notas Musicales Voladoras (`state === 'playing'`)**:
   Mientras suena el cuento, un bucle de tiempo (`setInterval`) inyecta notas de música e iconos mágicos (`🎵`, `🎶`, `♩`, `♪`, `✨`) desde el centro del libro. Utilizando variables de CSS personalizadas (`--x`, `--y`, `--rot`), las notas se elevan físicamente flotando y girando hacia los lados mediante transiciones fluidas.
4. **Celebración de Corazones al Guardar**:
   Al presionar el botón de corazón, la pantalla se inunda con un festival de confeti de corazones de colores que brotan desde el fondo del libro y ascienden rotando suavemente por el aire (`celebrationHearts`).

---

### 🎙️ Componentes Técnicos Clave

#### A. Captura de Audio Novedosa (`MediaRecorder`)
Captura la voz del micrófono en formato binario ligero (`audio/webm`):
```javascript
navigator.mediaDevices.getUserMedia({ audio: true })
    .then(stream => {
        this.mediaRecorder = new MediaRecorder(stream, { mimeType: 'audio/webm' });
        this.audioChunks = [];
        this.mediaRecorder.ondataavailable = e => this.audioChunks.push(e.data);
        this.mediaRecorder.onstop = () => this.uploadAudio();
        this.mediaRecorder.start();
    });
```

#### B. Protocolo de Transmisión SignalR + MessagePack
Para transferir de forma óptima los cuentos en tiempo real, el cliente SignalR de Microsoft se conecta con el protocolo binario **MessagePack** en lugar del tradicional JSON. Esto reduce drásticamente el peso de los mensajes binarios de audio:
```javascript
this.connection = new signalR.HubConnectionBuilder()
    .withUrl("/storyHub")
    .withHubProtocol(new signalR.protocols.msgpack.MessagePackHubProtocol())
    .build();
```

#### C. Cola de Reproducción FIFO (`playbackQueue`)
A medida que el backend genera el cuento, envía fragmentos de audio (`audioChunk`). El frontend los recibe asíncronamente y los acumula en una cola (First-In, First-Out). Una función recursiva encadenada los reproduce secuencialmente mediante eventos de audio HTML5 para simular un discurso continuo sin cortes:
```javascript
this.connection.on("audioChunk", (data) => {
    const blob = new Blob([data], { type: 'audio/wav' });
    this.playbackQueue.push(blob);
    if (!this.isPlaying) this.playNext();
});

// En el método playNext():
const blob = this.playbackQueue.shift();
const url = URL.createObjectURL(blob);
const audio = new Audio(url);
audio.play();
audio.onended = () => {
    URL.revokeObjectURL(url);
    this.playNext(); // Reproduce el siguiente fragmento en cola
};
```

---

## 🛠️ 2. Panel de Control y Edición (Backoffice)

El Backoffice (`backoffice/index.html`) ofrece una interfaz de gestión sofisticada con diseño de grado profesional y un tema oscuro elegante e inmersivo.

```
┌──────────────────────────────────────────────────────────────┐
│  Brand (Admin)                   StoryForge Editor           │
│  [🔍 Buscar cuentos...   ]       [Título del cuento] [Emojis]│
│  ┌──────────────────────┐       ┌──────────────────────────┐│
│  │ Cuento de Dragón     │       │ Párrafo 1  [🔊][▲][▼][🗑️]││
│  │ 🐉✨🏰 23 May        │       │ Texto editable del ...   ││
│  ├──────────────────────┤       ├──────────────────────────┤│
│  │ Cuento de Piratas    │       │ Párrafo 2  [🔊][▲][▼][🗑️]││
│  │ 🏴‍☠️⚓🦜 22 May        │       │ Texto editable del ...   ││
│  └──────────────────────┘       └──────────────────────────┘│
│                                  [🗑️ Eliminar]   [💾 Guardar]│
└──────────────────────────────────────────────────────────────┘
```

### 🧩 Elementos Clave del Panel de Control

1. **Diseño Oscuro Premium (Backoffice CSS)**:
   Inspirado en los mejores estándares modernos de SaaS. Implementa **Glassmorphism** (desenfoques de fondo en capas mediante `backdrop-filter: blur(16px)`), bordes translúcidos con brillos sutiles, fuentes elegantes (Inter/System UI) y transiciones fluidas de 0.3s en cada interacción.
2. **Barra de Búsqueda Reactiva**:
   Alpine.js filtra la lista en tiempo real en memoria a través de la función reactiva `filteredStories()`. Se puede buscar tanto por el título del cuento como por los emojis del tono.
3. **Consola del Cuentacuentos (Editor Multicapa)**:
   Al seleccionar un cuento, se genera una copia limpia del estado (`selectedStoryCopy`) mediante `JSON.parse(JSON.stringify(story))`. Esto garantiza que los cambios del formulario de edición sean seguros y se puedan descartar en cualquier momento sin alterar el estado real hasta que se haga clic en guardar.
4. **Tarjetas de Párrafos Inteligentes**:
   Cada párrafo del cuento se renderiza dentro de una tarjeta interactiva que permite:
   * **Reordenamiento Dinámico**: Subir (`▲`) o bajar (`▼`) párrafos del cuento alterando el orden del array de Alpine instantáneamente.
   * **Borrado Individual**: Remover párrafos específicos con el botón de papelera.
   * **Añadido Rápido**: Botón inferior para sumar cajas de texto vacías en cualquier punto.
   * **Narrador de Párrafo Individual (Síntesis de Voz instantánea)**: Al pulsar el botón `🔊`, el frontend realiza un `POST /api/stories/tts` enviando únicamente el texto de ese párrafo. El servidor responde con un streaming de voz WAV y el reproductor de HTML5 lo emite inmediatamente, mostrando una animación de micrófono activo en la tarjeta.
5. **Notificaciones Toast Flotantes**:
   Un sistema dinámico de notificaciones en la esquina inferior derecha. Al guardar, borrar o fallar una operación, se inyecta un banner reactivo (`toast-success` o `toast-error`) que desaparece con un desvanecimiento controlado a los 3.5 segundos.
6. **Responsividad Inteligente (Modo Retrato)**:
   El archivo `backoffice.css` está equipado con Media Queries avanzadas. En pantallas móviles y tablets (menores a 768px), el diseño de doble panel (sidebar + editor) se divide en dos vistas tipo slide utilizando la clase de Alpine `currentView === 'list' ? 'aside-visible' : 'main-visible'`. Un botón superior de retorno permite a los administradores navegar cómodamente desde sus móviles.
