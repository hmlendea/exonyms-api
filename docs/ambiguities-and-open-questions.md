# Ambiguities and Open Questions

## Overview

This document captures known ambiguities, design decisions that need clarification, and open questions about the Exonyms API. These items should be resolved to improve maintainability and guide future development.

## Data Source Ambiguities

### WikiData Sitelink Language Extraction

**Issue**: The regex `@"(news|quote|source|voyage|wiki)"` for extracting language codes from sitelinks is heuristic and may produce incorrect codes.

**Examples**:
- `dewiki` → `de` ✓
- `enwikinews` → `en` ✓
- `frwikiquote` → `fr` ✓
- `ruwikisource` → `ru` ✓
- `devoyage` → `de` ✓
- `simplewiki` → `simple` ? (not a standard ISO code)
- `be_x_oldwiki` → `be_x_old` ? (not a standard ISO code)

**Questions**:
1. Should we use WikiData's official language codes instead?
2. How to handle non-standard sitelinks?
3. Should we validate extracted codes against ISO 639-1?

**Proposed Solution**: Use WikiData's `wbgetentities` API with `props=labels|sitelinks` and parse language codes from the structured response.

### GeoNames Language Codes

**Issue**: GeoNames uses ISO 639-1 codes but also includes non-standard codes like `link`, `unlc`, `wkdt`.

**Current Handling**: Hardcoded ignore list.

**Questions**:
1. Are there other non-standard codes we should ignore?
2. Should we map GeoNames codes to standard ISO 639-1?
3. How to handle deprecated language codes?

### Transliteration Language Support

**Issue**: The transliteration API supports 68 languages, but the list is hardcoded in `TransliterationApiClient.LanguageCodes`.

**Questions**:
1. How to keep this list in sync with the external API?
2. What happens when the external API adds/removes languages?
3. Should we query the API for supported languages?

## Processing Pipeline Ambiguities

### Fallback Logic

**Issue**: The fallback logic has a complex double-transliteration step:

```csharp
string transliterated = transliterationApiClient.Transliterate(targetLang, fallbackName.OriginalValue);
if (transliterated == fallbackName.OriginalValue)
{
    transliterated = transliterationApiClient.Transliterate(fallbackLang, fallbackName.OriginalValue);
}
```

**Questions**:
1. What is the exact intent of this logic?
2. When does the first transliteration return the original value?
3. Is this handling the case where the target language uses the same script as the fallback?

**Documentation Needed**: Clear explanation of the fallback algorithm with examples.

### Construction Rules

**Issue**: German Middle High German (gmh) construction uses 70+ regex patterns that are not documented.

**Questions**:
1. What is the linguistic basis for these rules?
2. Are they complete/accurate?
3. How to validate correctness?
4. Should we support other historical languages?

### Normalisation Patterns

**Issue**: NameNormaliser has 100s of regex patterns across 50+ languages and 15+ categories.

**Questions**:
1. How were these patterns derived?
2. Are they maintained/updated?
3. How to test coverage?
4. Should we use a standard library instead?

## Architecture Ambiguities

### Singleton vs Transient Services

**Issue**: `ExonymsService`, `GeoNamesGatherer`, `WikiDataGatherer` are registered as Singletons but create new `HttpClient` instances per call.

**Questions**:
1. Why Singleton if they create per-call resources?
2. Should they be Transient instead?
3. Should we use `IHttpClientFactory`?

### Blocking Calls

**Issue**: Controller uses `.Result` on async method:

```csharp
Location exonyms = exonymsService.Gather(...).Result;
```

**Questions**:
1. Why not use `await`?
2. Is this causing thread pool starvation?
3. What is the impact under load?

### No Caching

**Issue**: Every request hits external APIs with no caching.

**Questions**:
1. Is this intentional?
2. What is the expected QPS?
3. What are the external API rate limits?
4. Should we add caching (Redis, in-memory)?

## Configuration Ambiguities

### GeoNames Usernames

**Issue**: 60+ usernames hardcoded in source code.

**Questions**:
1. Are these valid/active accounts?
2. How were they obtained?
3. What happens when they're exhausted?
4. Should we use a single account with higher limits?

### Transliteration API

**Issue**: External transliteration API URL is configurable but no authentication is configured.

**Questions**:
1. Does the API require authentication?
2. What are the rate limits?
3. Is there an SLA?
4. What happens if the API is unavailable?

## API Design Ambiguities

### Single Endpoint

**Issue**: Only one endpoint `/Exonyms` with optional parameters.

**Questions**:
1. Should we support batch requests?
2. Should we add search by name?
3. Should we add autocomplete/suggestions?
4. Should we support reverse lookup (name → IDs)?

### Response Format

**Issue**: Response includes both `originalValue` and `value` for each name.

**Questions**:
1. Do clients need both?
2. Should we add metadata (source, confidence)?
3. Should we support filtering by source?

### Versioning

**Issue**: No API versioning strategy.

**Questions**:
1. When will we need versioning?
2. What versioning scheme (URL, header, query)?
3. How to handle breaking changes?

## Testing Ambiguities

### Integration Tests

**Issue**: No integration tests for external APIs.

**Questions**:
1. Should we add contract tests for external APIs?
2. How to handle external API changes?
3. Should we record/replay HTTP interactions?

### Load Testing

**Issue**: No load testing or performance benchmarks.

**Questions**:
1. What is the expected load?
2. What are the latency requirements?
3. How does the system behave under load?

## Deployment Ambiguities

### Docker

**Issue**: No Dockerfile in repository.

**Questions**:
1. Is Docker the target deployment method?
2. What base image should we use?
3. How to handle configuration in containers?

### Health Checks

**Issue**: No health check endpoints.

**Questions**:
1. What should health checks verify?
2. Should they check external API connectivity?
3. What is the expected response format?

### Monitoring

**Issue**: No metrics, tracing, or alerting.

**Questions**:
1. What metrics are important?
2. Should we use OpenTelemetry?
3. What alerting thresholds?

## Open Questions for Future Development

### Feature Requests

1. **Batch API**: Support multiple IDs in one request
2. **Search API**: Find locations by name
3. **Caching**: Add Redis/in-memory caching
4. **Rate Limiting**: Protect external APIs
5. **Circuit Breaker**: Handle external API failures gracefully
6. **Metrics**: Add Prometheus metrics
7. **Tracing**: Add OpenTelemetry distributed tracing
8. **Health Checks**: Add Kubernetes-ready health endpoints
9. **Docker**: Add Dockerfile and docker-compose
10. **Documentation**: Add OpenAPI/Swagger

### Technical Debt

1. **HttpClient Management**: Use `IHttpClientFactory`
2. **Async/Await**: Replace `.Result` with `await`
3. **Configuration**: Move hardcoded values to config
4. **Logging**: Add structured logging for all operations
5. **Testing**: Add integration and load tests
6. **Security**: Rotate HMAC key, add API authentication
7. **Validation**: Add comprehensive input validation
8. **Error Handling**: Add retry logic and circuit breakers

### Research Needed

1. **WikiData API**: Better language code extraction
2. **GeoNames API**: Official language code mapping
3. **Transliteration**: Alternative services, self-hosted options
4. **Normalisation**: Standard libraries (ICU, etc.)
5. **Construction**: Linguistic validation of gmh rules
6. **Performance**: Benchmarking and optimization

## Decision Log

| Date | Decision | Rationale | Status |
|------|----------|-----------|--------|
| 2026-10-08 | Use WikiData + GeoNames | Complementary data sources | Implemented |
| 2026-10-08 | WikiData precedence | Higher quality labels | Implemented |
| 2026-10-08 | HMAC signing | Response integrity | Implemented |
| 2026-10-08 | No caching | Simplicity, freshness | Implemented |
| 2026-10-08 | Singleton services | Stateless, shared config | Implemented |
| 2026-10-08 | Hardcoded GeoNames usernames | Quick setup | **Needs Fix** |

## Priority Matrix

| Item | Impact | Effort | Priority |
|------|--------|--------|----------|
| Move GeoNames usernames to config | High | Low | **Critical** |
| Replace `.Result` with `await` | High | Low | **Critical** |
| Use `IHttpClientFactory` | High | Medium | **High** |
| Add caching layer | High | Medium | **High** |
| Add health checks | Medium | Low | **High** |
| Add integration tests | Medium | Medium | **Medium** |
| Add OpenAPI/Swagger | Medium | Low | **Medium** |
| Add Dockerfile | Medium | Low | **Medium** |
| Add metrics/tracing | Medium | High | **Medium** |
| Document gmh construction rules | Low | High | **Low** |
| Validate normalisation patterns | Low | High | **Low** |

## Related Documentation

- [Architecture](../architecture.md)
- [Design Decisions](../design-decisions.md)
- [Change Guide](../change-guide.md)
- [Security](../security.md)
- [Build and Deployment](../build-and-deployment.md)