# Dependencies

## External Dependencies (NuGet)

### Runtime Dependencies

| Package | Version | Purpose | License |
|---------|---------|---------|---------|
| `Newtonsoft.Json` | 13.0.4 | WikiData JSON parsing | MIT |
| `NuciAPI` | 3.6.2 | Core API abstractions (requests, responses, middleware) | MIT |
| `NuciAPI.Client` | 1.2.4 | HTTP client abstractions for NuciAPI | MIT |
| `NuciAPI.Controllers` | 2.3.2 | Base controller classes (NuciApiController) | MIT |
| `NuciAPI.Middleware.ExceptionHandling` | 1.0.3 | Exception handling middleware | MIT |
| `NuciAPI.Middleware.Logging` | 1.0.3 | Request logging middleware | MIT |
| `NuciAPI.Middleware.Security` | 1.0.6 | Security middleware | MIT |
| `NuciLog` | 1.2.3 | Structured logging implementation | MIT |
| `NuciLog.Core` | 3.1.1 | Logging abstractions (ILogger, Operation, LogInfoKey) | MIT |
| `NuciSecurity.HMAC` | 4.1.5 | HMAC response signing | MIT |
| `NuciWeb.HTTP` | 1.7.3 | HttpClient factory, creator utilities | MIT |

### Test Dependencies

| Package | Version | Purpose | License |
|---------|---------|---------|---------|
| `Microsoft.NET.Test.Sdk` | 18.11.0 | Test runner infrastructure | MIT |
| `Moq` | 4.21.0 | Mocking framework | BSD-3-Clause |
| `NUnit` | 5.0.0 | Test framework | MIT |
| `NUnit3TestAdapter` | 6.3.1 | VS Test Platform adapter | MIT |
| `TngTech.ArchUnitNET.NUnit` | 0.13.5 | Architecture fitness tests | Apache-2.0 |

## Internal Dependencies

### Project References

```
ExonymsAPI.UnitTests → ExonymsAPI
```

### Namespace Dependencies (within ExonymsAPI)

```
ExonymsAPI (host)
    └── ExonymsAPI.API.Controllers
    └── ExonymsAPI.API.Requests
    └── ExonymsAPI.API.Responses
    └── ExonymsAPI.Client.TransliterationAPI
    └── ExonymsAPI.Configuration
    └── ExonymsAPI.Logging
    └── ExonymsAPI.Service
            └── ExonymsAPI.Service.Gatherers
            └── ExonymsAPI.Service.Models
            └── ExonymsAPI.Service.Processors
```

## External Service Dependencies

| Service | Endpoint | Protocol | Auth | Purpose |
|---------|----------|----------|------|---------|
| GeoNames | `http://api.geonames.org/get` | HTTP GET | Username (query param) | Alternate names in XML |
| WikiData | `https://wikidata.org/wiki/Special:EntityData/{id}.json` | HTTP GET | None | Labels + sitelinks in JSON |
| Transliteration API | `{TransliterationApiBaseUrl}/Transliteration` | HTTP GET | None | Script → Latin conversion |

### GeoNames

- **Format**: XML
- **Parameters**: `geonameId`, `username`
- **Response**: `<geoname>` with `<name>` and `<alternateName lang="...">` elements
- **Rate limit**: Free tier, per-username limits
- **Usernames**: Hardcoded list of 60+ accounts, randomly selected per request

### WikiData

- **Format**: JSON (EntityData special page)
- **Parameters**: Entity ID in path (`Q123`)
- **Response**: `entities.{id}.labels` + `entities.{id}.sitelinks`
- **Rate limit**: Public API, generous limits
- **No authentication required**

### Transliteration API

- **Format**: JSON (NuciAPI envelope)
- **Parameters**: `Text`, `Language` (query)
- **Response**: `{ "Text": "transliterated" }`
- **Base URL**: Configured via `TransliterationSettings.TransliterationApiBaseUrl`
- **Client**: `NuciApiClient` with `GetTransliterationsRequest/Response`

## Dependency Graph

```
ExonymsAPI
├── Microsoft.AspNetCore.* (framework)
├── Microsoft.Extensions.* (DI, config, logging, hosting)
├── Newtonsoft.Json
├── NuciAPI
│   ├── NuciAPI.Client
│   ├── NuciAPI.Controllers
│   ├── NuciAPI.Middleware.ExceptionHandling
│   ├── NuciAPI.Middleware.Logging
│   └── NuciAPI.Middleware.Security
├── NuciLog
│   └── NuciLog.Core
├── NuciSecurity.HMAC
└── NuciWeb.HTTP

ExonymsAPI.UnitTests
├── ExonymsAPI
├── Moq
├── NUnit
└── NUnit3TestAdapter

ExonymsAPI.ArchitectureTests
├── ExonymsAPI
├── Microsoft.NET.Test.Sdk
├── NUnit
├── NUnit3TestAdapter
└── TngTech.ArchUnitNET.NUnit
```

## Version Constraints

- **Target Framework**: net10.0
- **NuciAPI ecosystem**: All packages from hmlendea organisation, version-locked
- **Newtonsoft.Json**: Pinned to 13.0.4 (avoids System.Text.Json conflicts)
- **Test packages**: Latest compatible with net10.0 at time of writing

## Transitive Dependencies of Note

- `NuciLog.Core` → `System.Text.Json` (for structured log serialisation)
- `NuciWeb.HTTP` → `System.Net.Http` (HttpClient)
- `NuciSecurity.HMAC` → `System.Security.Cryptography` (HMAC-SHA256)

## Security Considerations

- **GeoNames usernames**: Hardcoded in source (GeoNamesGatherer.cs:30-33) — consider moving to configuration/secrets
- **HMAC signing key**: Placeholder in appsettings.json (`[[EXONYMS_API_HMAC_SIGNING_KEY]]`) — must be replaced at deploy
- **Transliteration API URL**: Configured in appsettings.json — validate in production
- **No user data processed**: Only location identifiers (public IDs)