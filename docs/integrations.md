# Integrations

## Overview

The Exonyms API integrates with multiple external systems to gather, process, and deliver exonyms (alternative names for places in different languages). This document describes all external integrations, their contracts, and error handling.

## Integration Matrix

| Integration | Purpose | Protocol | Frequency | Error Handling |
|-------------|---------|----------|-----------|----------------|\n| GeoNames API | Source of alternate names | HTTP GET | Per request | Retry on failure |
| WikiData API | Source of labels and sitelinks | HTTP GET | Per request | Retry on failure |
| Transliteration API | Script conversion (non-Latin → Latin) | HTTP GET | Per name | Graceful degradation |
| NuciAPI | API framework (controllers, logging, security) | Internal | Per request | Framework exceptions |
| NuciLog | Structured logging | Internal | Per request | Framework exceptions |
| NuciSecurity.HMAC | Response signing | Internal | Per response | Framework exceptions |

## GeoNames Integration

### Contract

**Endpoint**: `http://api.geonames.org/get`

**Parameters**:
| Parameter | Type | Required | Description |
|-----------|------|----------|-------------|
| `geonameId` | `int` | Yes | GeoNames numeric identifier |
| `username` | `string` | Yes | Rotated from 60+ hardcoded accounts |

**Example**: `http://api.geonames.org/get?geonameId=310350&username=geonamesfreeaccountt`

### Response Format

**Content-Type**: `text/xml; charset=utf-8`

**Schema**:
```xml
<geoname>
  <name>Al Hoceima</name>
  <alternateName lang="ar">الحسيمة</alternateName>
  <alternateName lang="de">Alhucemas</alternateName>
  <alternateName lang="fr">Al Hoceïma</alternateName>
  <alternateName lang="link">http://...</alternateName>
  <alternateName lang="unlc">...</alternateName>
  <alternateName lang="wkdt">...</alternateName>
</geoname>
```

### Integration Details

**Implementation**: `GeoNamesGatherer.Gather()` in `ExonymsAPI/Service/Gatherers/GeoNamesGatherer.cs`

**Key Features**:
- Random username rotation from 60+ accounts
- XML parsing with `XDocument.Parse()`
- Language filtering (ignores `link`, `unlc`, `wkdt`)
- Name processing pipeline (transliteration + normalisation)

**Error Scenarios**:
- HTTP non-2xx → `HttpRequestException` → 500
- XML parse error → `XmlException` → 500
- Missing `<name>` → Null reference → 500

### Configuration

**Hardcoded**: Usernames in source code (security consideration)

**Recommendation**: Move to configuration/secrets in future.

## WikiData Integration

### Contract

**Endpoint**: `https://wikidata.org/wiki/Special:EntityData/{wikiDataId}.json`

**Parameters**:
| Parameter | Type | Required | Description |
|-----------|------|----------|-------------|
| `wikiDataId` | `string` | Yes | WikiData entity ID (e.g., `Q310350`) |

**Example**: `https://wikidata.org/wiki/Special:EntityData/Q310350.json`

### Response Format

**Content-Type**: `application/json`

**Schema**:
```json
{
  "entities": {
    "Q310350": {
      "labels": {
        "en": { "value": "Al Hoceima" },
        "ar": { "value": "الحسيمة" },
        "de": { "value": "Alhucemas" }
      },
      "sitelinks": {
        "dewiki": { "title": "Alhucemas" },
        "enwiki": { "title": "Al Hoceima" }
      }
    }
  }
}
```

### Integration Details

**Implementation**: `WikiDataGatherer.Gather()` in `ExonymsAPI/Service/Gatherers/WikiDataGatherer.cs`

**Key Features**:
- JSON parsing with `JObject.Parse()`
- Label extraction (all languages)
- Sitelink parsing with language code extraction via regex
- Name processing pipeline (transliteration + normalisation)

**Language Extraction**:
```csharp
string languageCode = Regex.Replace(sitelink.Key, @"(news|quote|source|voyage|wiki)", "");
```

**Error Scenarios**:
- HTTP non-2xx → `HttpRequestException` → 500
- JSON parse error → `JsonReaderException` → 500
- Missing entity → Empty Location (silent)
- Missing labels → Empty Names (silent)

## Transliteration Integration

### Contract

**Endpoint**: External transliteration service via `NuciApiClient`

**Parameters**:
| Parameter | Type | Required | Description |
|-----------|------|----------|-------------|
| `Text` | `string` | Yes | Name to transliterate |
| `Language` | `string` | Yes | Source language code (e.g., `ru`, `ar`, `ja`) |

**Example Request**:
```json
{
  "request": { "Text": "Москва", "Language": "ru" },
  "requestId": "guid",
  "timestamp": "2026-10-08T12:00:00Z"
}
```

**Example Response**:
```json
{
  "success": true,
  "response": { "Text": "Moskva" },
  "requestId": "guid",
  "timestamp": "2026-10-08T12:00:00Z"
}
```

### Integration Details

**Implementation**: `TransliterationApiClient.Transliterate()` in `ExonymsAPI/Client/TransliterationAPI/TransliterationApiClient.cs`

**Key Features**:
- Uses `NuciApiClient` for HTTP + envelope handling
- Supports 68 languages (see `LanguageCodes`)
- Graceful degradation on errors (returns original text)
- Configured via `TransliterationSettings.TransliterationApiBaseUrl`

**Error Scenarios**:
- Language not supported → Return original (no error)
- API HTTP error → Return original (handled by NuciApiClient)
- API unsuccessful envelope → Return original (handled by NuciApiClient)
- Empty response.Text → Return original

## NuciAPI Ecosystem Integration

### Components

| Component | Purpose | Integration Type |
|-----------|---------|------------------|
| NuciAPI.Controllers | Controller base classes, middleware | Internal dependency |
| NuciAPI.Middleware | Request logging, exception handling | Internal dependency |
| NuciLog | Structured logging with MyOperation/MyLogInfoKey | Internal dependency |
| NuciSecurity.HMAC | Response signing | Internal dependency |
| NuciWeb.HTTP | HTTP client with retry policies | Internal dependency |

### Integration Details

**Dependency Injection**:
- Configured in `ServiceCollectionExtensions.cs`
- Singleton services: Business logic, gatherers, processors
- Transient services: HTTP clients, loggers

**Middleware Pipeline**:
1. `UseNuciApiRequestLogging` - Structured request/response logging
2. `UseNuciApiExceptionHandling` - Standardized error responses
3. Standard ASP.NET Core middleware (HTTPS, routing, etc.)

## Integration Test Considerations

### Current State

**No integration tests** for external APIs:
- No mocks for GeoNames HTTP calls
- No mocks for WikiData HTTP calls
- No mocks for Transliteration API calls

### Recommended Test Strategy

**Unit Tests (existing)**:
- Mock `IGeoNamesGatherer`, `IWikiDataGatherer`
- Mock `ITransliterationApiClient`
- Mock `INameNormaliser`, `INameConstructor`

**Integration Tests (future)**:
- Use `HttpClientFactory` with `HttpMessageHandler`
- Mock external HTTP responses
- Test end-to-end pipeline with real services

### Test Coverage Gaps

| Integration | Test Coverage | Gap |
|-------------|---------------|-----|
| GeoNames | None | No HTTP mocking |
| WikiData | None | No HTTP mocking |
| Transliteration | None | No HTTP mocking |
| NuciAPI | Full (via unit tests) | Framework integration tested |

## Integration Monitoring

### Health Checks

**Not implemented**. External API failures result in 500 responses.

### Monitoring Recommendations

**Metrics to track**:
- External API call success/failure rates
- Response latency per external service
- Error types and frequencies
- Username rotation usage (GeoNames)

**Implementation approach**:
- Add health check endpoints for external services
- Log external API errors with correlation IDs
- Implement circuit breaker pattern for repeated failures

## Integration Security

### GeoNames

**Risk**: Hardcoded credentials in source code

**Mitigation**:
- Move usernames to configuration/secrets
- Implement rate limiting per username
- Monitor for abuse

### WikiData

**Risk**: No authentication required

**Mitigation**:
- Implement respectful rate limiting
- Cache responses to reduce calls
- Monitor for excessive usage

### Transliteration API

**Risk**: External service dependency

**Mitigation**:
- Graceful degradation on failures
- Fallback to original text
- Monitor service availability

## Integration Evolution

### Future Enhancements

1. **Configuration-driven credentials**
   - Move GeoNames usernames to configuration
   - Add API keys for external services

2. **Caching layer**
   - Cache external API responses
   - Reduce external calls per request

3. **Circuit breaker pattern**
   - Detect and handle external service failures
   - Provide fallback behavior

4. **Health checks**
   - Monitor external service availability
   - Alert on degradation

5. **Rate limiting**
   - Implement server-side rate limiting
   - Respect external service limits

### Breaking Changes

**External API contracts**:
- GeoNames endpoint format (unlikely to change)
- WikiData response format (unlikely to change)
- Transliteration API contract (depends on external service)

**Internal contracts**:
- Service interfaces (`IGatherer`, `IProcessor`)
- Response models (`Location`, `Name`)

## Integration Documentation

### API Reference

- [Exonyms Controller](api-reference/exonyms-controller.md) - Main endpoint
- [Error Models](api-reference/error-models.md) - Error response formats

### Technical Documentation

- [Components: Integration Models](components/integration-models.md) - Detailed integration models
- [Flows: Exonym Gathering](flows/exonym-gathering.md) - End-to-end data flow
- [Configuration](configuration.md) - Integration configuration
- [Security](security.md) - Security considerations

### Development Guidelines

- Follow existing patterns in `ServiceCollectionExtensions.cs`
- Use `NuciApiClient` for external HTTP calls
- Implement graceful degradation for external failures
- Log all external API interactions
- Monitor external service health

## Integration Summary

The Exonyms API successfully integrates with multiple external systems to provide comprehensive exonym data. Key characteristics:

- **Multiple sources**: WikiData (labels) + GeoNames (alternate names)
- **Processing pipeline**: Transliteration → Normalisation → Construction → Fallbacks
- **Error resilience**: Graceful degradation on external failures
- **No caching**: Every request hits external APIs
- **Hardcoded credentials**: Security consideration for future improvement

Integration strategy balances comprehensive data coverage with resilience to external service failures.