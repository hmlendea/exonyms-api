# Invariants

## Overview

Invariants are conditions that must always hold true in the Exonyms API. They define the guarantees the system provides to clients and the constraints the code must maintain.

## API Response Invariants

### Success Response Structure

**Invariant**: Every successful response (HTTP 200) has the following structure:

```json
{
  "success": true,
  "defaultName": "<string>",
  "names": { "<lang>": { ... } },
  "count": <integer>,
  "hmac": "sha256=<base64>"
}
```

**Guarantees**:
- `success` is always `true`
- `defaultName` is always a non-null string
- `names` is always a non-null object (may be empty)
- `count` equals `names.Count`
- `hmac` is always present and valid

### Error Response Structure

**Invariant**: Every error response (HTTP 4xx/5xx) has the following structure:

```json
{
  "success": false,
  "error": {
    "code": "<string>",
    "message": "<string>"
  },
  "requestId": "<guid>"
}
```

**Guarantees**:
- `success` is always `false`
- `error.code` is always present
- `error.message` is always present
- `requestId` is always present

## Name Processing Invariants

### Transliteration Invariant

**Invariant**: If a language is in the transliteration list, the output value is in Latin script.

**Exception**: If the transliteration API fails, the original value is returned unchanged.

### Normalisation Invariant

**Invariant**: The output value never contains:
- Administrative suffixes (e.g., "City of", "County of")
- Prefixes (e.g., "The", "El", "La")
- Quotes, XML tags, or other markup
- Leading/trailing whitespace
- Multiple consecutive spaces or dashes

### Construction Invariant

**Invariant**: Constructed names (e.g., `gmh`) always have a `comment` field explaining the base language.

### Fallback Invariant

**Invariant**: Fallback names always have a `comment` field explaining the source language.

## Merge Invariants

### WikiData Precedence

**Invariant**: When both WikiData and GeoNames provide a name for the same language, WikiData's value is used.

### Default Name Invariant

**Invariant**: `defaultName` is always from WikiData if available, otherwise from GeoNames.

## Deduplication Invariant

**Invariant**: No name in `names` has a `value` equal to `defaultName` (case-insensitive).

## Sorting Invariant

**Invariant**: `names` is always sorted alphabetically by language code.

## Language Code Invariants

### ISO 639-1

**Invariant**: All language codes are 2-letter ISO 639-1 codes.

**Exception**: `gmh` (German Middle High German) is a 3-letter code.

### Valid Codes

**Invariant**: All language codes in the response are valid ISO 639-1 codes.

## Data Model Invariants

### Location Model

**Invariant**: `Location` always has:
- `DefaultName` (non-null string)
- `Names` (non-null dictionary)

### Name Model

**Invariant**: `Name` always has:
- `OriginalValue` (non-null string)
- `Value` (non-null string)
- `Comment` (nullable string)

## Service Invariants

### Gather Invariant

**Invariant**: `ExonymsService.Gather()` always returns a valid `Location` object (never null).

**Exception**: If both `geoNamesId` and `wikiDataId` are null/empty, the service throws an exception.

### Thread Safety Invariant

**Invariant**: Concurrent calls to `ExonymsService.Gather()` never corrupt shared state.

**Implementation**: Each call creates new `Location` objects; no shared mutable state.

## HTTP Invariants

### Method Invariant

**Invariant**: The `/Exonyms` endpoint only accepts `GET` requests.

### Authorization Invariant

**Invariant**: No authentication or authorization is required for the `/Exonyms` endpoint.

### Idempotency Invariant

**Invariant**: Multiple identical requests produce identical responses.

## Performance Invariants

### Latency Invariant

**Invariant**: Typical response latency is under 1 second (95th percentile).

**Exception**: External API failures or slow responses may exceed this.

### Availability Invariant

**Invariant**: The API is available 24/7 (subject to external service availability).

## Security Invariants

### HMAC Invariant

**Invariant**: Every successful response is signed with HMAC-SHA256.

**Guarantee**: Clients can verify response integrity using the shared HMAC key.

### No Secrets in Logs Invariant

**Invariant**: No secrets (HMAC keys, API credentials) are ever logged.

## Error Handling Invariants

### Exception Propagation Invariant

**Invariant**: Unhandled exceptions are always caught by the middleware and converted to error responses.

**Guarantee**: No unhandled exceptions reach the client.

### Graceful Degradation Invariant

**Invariant**: Transliteration failures never cause the request to fail.

**Guarantee**: If transliteration fails, the original value is returned.

## Configuration Invariants

### Required Settings

**Invariant**: The following settings are always required:
- `TransliterationSettings.TransliterationApiBaseUrl`
- `SecuritySettings.HmacSigningKey`

### Default Values

**Invariant**: If a setting is missing, the application fails to start (no silent defaults).

## Testing Invariants

### Test Coverage Invariant

**Invariant**: All public methods have corresponding unit tests.

**Current Status**: Partially met (controller tests missing).

### Determinism Invariant

**Invariant**: Unit tests are deterministic (no external API calls).

**Implementation**: All external dependencies are mocked.

## Known Violations

### Current Violations

| Invariant | Violation | Mitigation |
|-----------|-----------|------------|
| Thread safety | `HttpClient` not disposed | Use `IHttpClientFactory` |
| Async | `.Result` blocking | Replace with `await` |
| Caching | No server-side caching | Add caching layer |
| Retry | No retry logic | Implement retry |
| Circuit breaker | No circuit breaker | Implement circuit breaker |

### Future Violations

| Invariant | Planned Violation | Reason |
|-----------|-------------------|--------|
| No secrets in logs | Log HMAC key for debugging | Development only |
| Determinism | Integration tests with real APIs | Test external contracts |

## Invariant Enforcement

### Runtime Enforcement

- **Assertions**: No runtime assertions in production code
- **Validation**: Input validation in controller
- **Logging**: All errors logged

### Compile-Time Enforcement

- **Type safety**: C# strong typing
- **Null safety**: Nullable reference types (if enabled)
- **Immutable**: `readonly` fields where possible

### Test Enforcement

- **Unit tests**: Verify invariants for each component
- **Integration tests**: Verify end-to-end invariants
- **Contract tests**: Verify API response format

## Monitoring Invariants

### Metrics

| Metric | Invariant | Alert Threshold |
|--------|-----------|-----------------|
| Error rate | < 1% | > 5% |
| Latency (p95) | < 1s | > 2s |
| Transliteration failures | < 5% | > 10% |
| External API failures | < 1% | > 5% |

### Health Checks

**Not implemented**. Recommended:
- External API health checks
- Memory usage checks
- Thread pool checks

## Related Documentation

- [Architecture](../architecture.md)
- [Components: Application Services](../components/application-services.md)
- [Error Handling](../error-handling.md)
- [Testing](../testing.md)
- [Security](../security.md)