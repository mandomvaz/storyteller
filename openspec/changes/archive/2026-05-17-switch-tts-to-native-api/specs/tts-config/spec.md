## MODIFIED Requirements

### Requirement: Configurable XTTS endpoint

The system SHALL read the XTTS v2 server URL from configuration under the `Xtts:Endpoint` key with a default value of `http://localhost:8020`, and use it directly as the base URL for an `HttpClient` (without the `/v1` suffix used previously).

#### Scenario: Default endpoint used when not configured
- **WHEN** the configuration key `Xtts:Endpoint` is not present
- **THEN** the `HttpClient` SHALL use `http://localhost:8020` as the base URL

#### Scenario: Custom endpoint from configuration
- **WHEN** the configuration key `Xtts:Endpoint` is set to `http://10.0.0.5:9020`
- **THEN** the `HttpClient` SHALL be configured with `http://10.0.0.5:9020` as the base URL

### Requirement: Configurable XTTS speaker

The system SHALL read the TTS speaker name from configuration under `Xtts:Speaker` (default `alloy`), passed as the `speaker_wav` parameter when calling `POST /tts_to_audio/`.

#### Scenario: Default speaker used when not configured
- **WHEN** `Xtts:Speaker` is not configured
- **THEN** the system SHALL default to `alloy` as the speaker

#### Scenario: Custom speaker from configuration
- **WHEN** `Xtts:Speaker` is set to `female`
- **THEN** the system SHALL use `female` as the `speaker_wav` parameter

### Requirement: Configurable XTTS language

The system SHALL read the TTS language code from configuration under `Xtts:Language` (default `es`), passed as the `language` parameter when calling `POST /tts_to_audio/`.

#### Scenario: Default language used when not configured
- **WHEN** `Xtts:Language` is not configured
- **THEN** the system SHALL default to `es` as the language

#### Scenario: Custom language from configuration
- **WHEN** `Xtts:Language` is set to `en`
- **THEN** the system SHALL use `en` as the `language` parameter

## REMOVED Requirements

### Requirement: Configurable XTTS model name

**Reason**: La API nativa de XTTS no expone el modelo en la petición; el servidor ya tiene el modelo cargado internamente.

**Migration**: Eliminar `Xtts:Model` de `appsettings.json`. No hay sustituto necesario.

### Requirement: API key placeholder

**Reason**: La API nativa de XTTS no requiere autenticación ni clave API, a diferencia de `OpenAIClient`.

**Migration**: Eliminar el `ApiKeyCredential` de `Program.cs`. No hay sustituto necesario.
