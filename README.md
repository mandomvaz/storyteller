# StoryForge 📖✨

Pipeline de voz a cuento en tiempo real potenciado por **Semantic Kernel**. Graba audio desde el navegador, lo transcribe con Whisper, genera un cuento personalizado usando Ollama (LLM) y sintetiza audio párrafo a párrafo en tiempo real con XTTS, entregando voz y texto de manera inmediata a la interfaz de usuario.

---

## 🌟 Características Actuales

- **Grabación Directa** — Captura de audio en tiempo real desde el navegador utilizando la API nativa `MediaRecorder` (WebM).
- **Transcripción Rápida (ASR)** — Conversión de voz a texto a través de servidores Whisper o Faster-Whisper integrados mediante Semantic Kernel (`IAudioToTextService`).
- **Generación de Cuentos Creativos** — Streaming de historias personalizadas a partir del texto transcrito mediante Ollama y modelos LLM localmente (`IChatCompletionService`).
- **Tubería de Audio Concurrente (TTS)** — Síntesis de voz párrafo a párrafo con XTTS-v2 en tiempo real. Un sistema concurrente basado en `System.Threading.Channels` procesa el texto a medida que se genera.
- **Transmisión Bidireccional de Alto Rendimiento** — Envío en tiempo real de fragmentos de audio y texto al frontend a través de **SignalR** utilizando el protocolo binario **MessagePack**.
- **Generador de Insignias Temáticas (Badges)** — Análisis automático del tono de la historia mediante una función semántica de Semantic Kernel que genera una combinación de 5 emojis representativos.
- **Persistencia en Base de Datos** — Almacenamiento local persistente de historias en una base de datos **SQLite** mediante un repositorio optimizado.
- **Historial Completo y Backoffice** — Panel de control moderno para explorar cuentos guardados, leerlos, editarlos, eliminarlos y reproducir su narración por voz en streaming en cualquier cliente activo.
- **Calentamiento Inteligente (Warmup Service)** — Servicio de precalentamiento con bloqueo concurrente para evitar llamadas duplicadas de inicialización del modelo de voz en el arranque del servidor.

---

## 🛠️ Stack Tecnológico

- **Backend**:
  - .NET 10 (Minimal APIs)
  - Semantic Kernel 1.76 (Abstracciones de IA de Microsoft)
  - SignalR con MessagePack (Comunicación en tiempo real de alto rendimiento)
  - SQLite (Motor de base de datos relacional ligero)
  - Microsoft.Data.Sqlite (Acceso a base de datos de baja latencia)
- **Frontends**:
  - **Lector Principal**: Alpine.js (SPA reactiva ultra ligera)
  - **Backoffice**: Alpine.js, Tailwind-inspired Vanilla CSS con diseño moderno, oscuro y adaptable.
- **Motores de IA externos**:
  - **Ollama** (Servidor local de modelos de lenguaje)
  - **Faster-Whisper** (Servidor local de transcripción de audio compatible con API OpenAI)
  - **XTTS API** (Servidor de síntesis de voz multilingüe de alta calidad)

---

## 📋 Requisitos Previos (Ejecución Local de Desarrollo)

Para ejecutar el proyecto directamente en tu PC de desarrollo sin Docker, necesitas:

1. **[.NET 10 SDK](https://dotnet.microsoft.com/download)** instalado.
2. **Ollama** ejecutándose localmente en `http://localhost:11434` (con el modelo correspondiente cargado).
3. **Faster-Whisper Server** en `http://localhost:8000`.
4. **XTTS API Server** en `http://localhost:8020`.

---

## 🚀 Inicio Rápido en Desarrollo

1. Dirígete a la carpeta del proyecto de código:
   ```bash
   cd storyforge
   ```
2. Ejecuta el servidor de desarrollo:
   ```bash
   dotnet run --launch-profile http
   ```
3. Abre las interfaces en tu navegador:
   - **Lector Principal (Grabadora y Reproductor)**: `http://localhost:5147`
   - **Panel de Control (Backoffice Histórico)**: `http://localhost:5147/backoffice/index.html`

---

## ⚙️ Configuración (`appsettings.json`)

Edita el archivo `appsettings.json` o define variables de entorno para ajustar los parámetros de funcionamiento:

| Clave | Valor por Defecto | Descripción |
|---|---|---|
| `Ollama:Endpoint` | `http://localhost:11434` | Endpoint del servidor de Ollama |
| `Ollama:TextModel` | `storyteller` | Modelo de lenguaje para generar cuentos |
| `Whisper:Endpoint` | `http://localhost:8000` | Endpoint de Faster-Whisper compatible con OpenAI |
| `Whisper:Model` | `medium` | Modelo Whisper a utilizar (ej. tiny, base, medium) |
| `Xtts:Endpoint` | `http://localhost:8020` | Endpoint del servidor XTTS |
| `Xtts:Speaker` | `laura.wav` | Muestra de voz de referencia para la clonación |
| `Xtts:Language` | `es` | Idioma de destino para la síntesis de voz |
| `Database:Path` | `Data/storyforge.db` | Ruta relativa o absoluta de la base de datos SQLite |
| `BadgePrompt:Path` | `Prompts/badge.txt` | Ruta del archivo de prompt para la generación de badges |

---

## 🐳 Despliegue en Producción (Docker)

Para desplegar la aplicación en un entorno real y de producción utilizando contenedores Docker y Docker Compose, consulta nuestra guía paso a paso y completamente detallada en castellano:

👉 **[Guía de Despliegue de StoryForge (DEPLOY.md)](DEPLOY.md)**

---

## 📐 Arquitectura del Sistema

Para un desglose técnico profundo del flujo de datos asíncrono, los canales de streaming (`System.Threading.Channels`), y los patrones de diseño utilizados en este proyecto, consulta el documento técnico de arquitectura:

👉 **[Arquitectura de StoryForge (ARCHITECTURE.md)](ARCHITECTURE.md)**

---

## ⚖️ Licencia y Disclaimer

### Disclaimer

**Este proyecto se entrega "tal cual", sin ningún tipo de garantía ni soporte.**  
Una vez finalizado, el autor no realizará actualizaciones adicionales, correcciones de errores, parches de seguridad ni añadirá nuevas características. No se revisarán, aceptarán ni fusionarán solicitudes de cambios (Pull Requests), modificaciones ni contribuciones externas. Si deseas mantener tu propia versión con cambios, eres libre de realizar un fork del repositorio bajo los términos de la licencia GPL-3.0.

### Licencia

[GNU General Public License v3.0](LICENSE)
