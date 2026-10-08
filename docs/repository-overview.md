# Repository Overview

## Purpose

The Exonyms API provides a REST endpoint to gather exonyms (foreign-language names) for geographical locations. Given a GeoNames ID and/or WikiData ID, it returns a structured list of names in multiple languages, all transliterated to Latin script.

## Scope

### In Scope
- Aggregating names from GeoNames and WikiData
- Transliteration to Latin script via external API
- Name normalisation (removing administrative suffixes like "City", "County", "River")
- Historical language construction (German Middle High German)
- Language fallbacks for missing exonyms
- HMAC-signed responses
- Stateless, on-demand processing

### Out of Scope
- Persistent storage or caching of results
- User authentication/authorisation (endpoint is public)
- Rate limiting (delegated to infrastructure)
- Batch processing or bulk exports
- Non-Latin script output

## Entry Points

### HTTP Endpoint
```
GET /Exonyms?geoNamesId={id}&wikiDataId={id}
```

- **Controller**: `ExonymsController.Get`
- **Action**: `ExonymsController.Get(GetExonymsRequest)`
- **Authentication**: None (public endpoint)
- **Authorisation**: None

### Program Entry
- `Program.Main()` → `CreateHostBuilder()` → `Startup`
- Standard ASP.NET Core generic host

## Key Capabilities

1. **Dual-source gathering**: Fetches from both GeoNames (XML) and WikiData (JSON) in parallel
2. **Transliteration**: Converts non-Latin scripts to Latin via external API
3. **Normalisation**: Strips administrative/geographic suffixes in 50+ languages
4. **Construction**: Generates historical language variants (currently gmh from de)
5. **Fallbacks**: Synthesises missing languages from related languages
6. **Deduplication**: Removes exonyms identical to default name
7. **Signing**: HMAC-SHA256 signs all responses

## External Integrations

| Service | Protocol | Format | Purpose |
|---------|----------|--------|---------|
| GeoNames | HTTP GET | XML | Primary name source |
| WikiData | HTTP GET | JSON | Primary name source + sitelinks |
| Transliteration API | HTTP GET | JSON | Script conversion to Latin |

## Configuration

All configuration via `appsettings.json`:
- `TransliterationSettings.TransliterationApiBaseUrl`
- `SecuritySettings.HmacSigningKey`
- `NuciLoggerSettings` (file path, output enablement)

## Technology Stack

- **Framework**: ASP.NET Core 10.0
- **DI**: Built-in Microsoft.Extensions.DependencyInjection
- **Logging**: NuciLog (structured, file + console)
- **HTTP Client**: NuciWeb.HTTP (with HttpClientCreator)
- **API Framework**: NuciAPI (controllers, middleware, requests/responses)
- **Security**: NuciSecurity.HMAC (response signing)
- **Testing**: NUnit 4.5, Moq 4.20
- **JSON**: Newtonsoft.Json (WikiData parsing)
- **XML**: System.Xml.Linq (GeoNames parsing)