# Configuration

## Configuration Schema

### appsettings.json Structure

```json
{
  "transliterationSettings": {
    "transliterationApiBaseUrl": "string (required)"
  },
  "securitySettings": {
    "hmacSigningKey": "string (required)"
  },
  "nuciLoggerSettings": {
    "logFilePath": "string (default: logfile.log)",
    "isFileOutputEnabled": "boolean (default: true)"
  }
}
```

### Settings Classes

| Class | Namespace | Properties |
|-------|-----------|------------|
| `TransliterationSettings` | `ExonymsAPI.Configuration` | `TransliterationApiBaseUrl` (string) |
| `SecuritySettings` | `ExonymsAPI.Configuration` | `HmacSigningKey` (string) |
| `NuciLoggerSettings` | `NuciLog.Configuration` | `LogFilePath` (string), `IsFileOutputEnabled` (bool) |

## Configuration Sources & Precedence

1. **appsettings.json** (base)
2. **appsettings.Development.json** (development overrides)
3. **Environment variables** (prefix: `EXONYMSAPI_` or standard ASP.NET Core)
4. **Command line arguments**
5. **User secrets** (development only)

ASP.NET Core configuration builder applies in order; later sources override earlier.

## Required Configuration

| Setting | Source | Description |
|---------|--------|-------------|
| `TransliterationSettings:TransliterationApiBaseUrl` | appsettings.json / env | Base URL for transliteration API (e.g., `https://api.nucilandia.ro/translit`) |
| `SecuritySettings:HmacSigningKey` | appsettings.json / env / secrets | HMAC-SHA256 signing key for response integrity |

## Optional Configuration

| Setting | Default | Description |
|---------|---------|-------------|
| `NuciLoggerSettings:LogFilePath` | `logfile.log` | Path to log file (relative to working directory) |
| `NuciLoggerSettings:IsFileOutputEnabled` | `true` | Enable file logging sink |

## Configuration Binding

In `ServiceCollectionExtensions.AddConfigurations()`:

```csharp
TransliterationSettings transliterationSettings = new();
SecuritySettings securitySettings = new();
NuciLoggerSettings nuciLoggerSettings = new();

configuration.Bind(nameof(TransliterationSettings), transliterationSettings);
configuration.Bind(nameof(SecuritySettings), securitySettings);
configuration.Bind(nameof(NuciLoggerSettings), nuciLoggerSettings);

services.AddSingleton(transliterationSettings);
services.AddSingleton(securitySettings);
services.AddSingleton(nuciLoggerSettings);
```

- Uses `IConfiguration.Bind()` with section name matching class name
- Registered as singletons for DI injection

## Environment-Specific Values

### Development (appsettings.Development.json)
- Typically overrides `NuciLoggerSettings:LogFilePath` or disables file output
- May use local transliteration endpoint

### Production
- `SecuritySettings:HmacSigningKey` **must** be set via environment variable or secrets manager
- `TransliterationSettings:TransliterationApiBaseUrl` points to production transliteration service
- `NuciLoggerSettings:IsFileOutputEnabled` typically `true` with persistent volume

## Secrets Management

**Never commit real secrets to source control.**

- `appsettings.json` contains placeholder: `[[EXONYMS_API_HMAC_SIGNING_KEY]]`
- Production key injected via:
  - Environment variable: `EXONYMSAPI_SecuritySettings__HmacSigningKey`
  - Azure Key Vault / AWS Secrets Manager / HashiCorp Vault
  - Kubernetes secrets
  - Docker secrets

## Validation

No explicit validation at startup. Missing configuration results in:
- `null` or default values in settings objects
- Runtime failures when settings are accessed (e.g., `ArgumentNullException` in `TransliterationApiClient` constructor)
- HMAC signing fails with empty key

**Recommendation**: Add startup validation in `Program.cs` or `Startup.ConfigureServices()`.

## Configuration Consumers

| Setting | Consumers |
|---------|-----------|
| `TransliterationApiBaseUrl` | `TransliterationApiClient` (via `NuciApiClient` constructor) |
| `HmacSigningKey` | `ExonymsController` → `GetExonymsResponse.SignHMAC()` |
| `NuciLoggerSettings` | `NuciLogger` (via `NuciLog` package internals) |

## Runtime Configuration Changes

- **Not supported**: Settings are bound at startup, registered as singletons
- **To change**: Restart application (or implement `IOptionsMonitor`/`IOptionsSnapshot` for hot reload)
- **Logging level**: Can be changed at runtime via NuciLog API if exposed