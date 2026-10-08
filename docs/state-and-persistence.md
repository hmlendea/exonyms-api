# State and Persistence

## State Management Overview

The Exonyms API is **stateless** by design. No in-process state is maintained between requests. All data is fetched on-demand from external sources for each request.

## In-Memory State

### Per-Request State

| Component | State | Lifetime |
|-----------|-------|----------|
| `ExonymsService.Gather()` | Local `Location` object, intermediate collections | Request scope |
| `GeoNamesGatherer.FetchLocation()` | `HttpClient`, XML parsing intermediates | Method scope |
| `WikiDataGatherer.FetchLocation()` | `HttpClient`, JSON parsing intermediates | Method scope |
| `TransliterationApiClient.Transliterate()` | `NuciApiClient`, request/response objects | Method scope |

### Singleton Service State

The following services are registered as singletons but hold **no mutable state**:

| Service | Interface | State |
|---------|-----------|-------|
| `ExonymsService` | `IExonymsService` | None (stateless orchestration) |
| `GeoNamesGatherer` | `IGeoNamesGatherer` | None (usernames `readonly`) |
| `WikiDataGatherer` | `IWikiDataGatherer` | None |
| `NameNormaliser` | `INameNormaliser` | None (pure functions) |
| `NameConstructor` | `INameConstructor` | None (readonly transformation dictionary) |

**Thread-safety**: All singleton services are immutable after construction. Safe for concurrent access.

### Transient Service State

| Service | Interface | State |
|---------|-----------|-------|
| `TransliterationApiClient` | `ITransliterationApiClient` | None (wraps `NuciApiClient`) |
| `NuciApiClient` | `INuciApiClient` | `HttpClient` (per-instance, thread-safe) |

## External State

### GeoNames

- **State location**: GeoNames servers
- **Data**: Geographical database with alternate names
- **Consistency**: Eventual (updates propagated periodically)
- **Access**: Read-only via HTTP GET
- **Caching**: None in this application

### WikiData

- **State location**: WikiData/Wikimedia servers
- **Data**: Structured knowledge base with multilingual labels
- **Consistency**: Eventual (community edits)
- **Access**: Read-only via HTTP GET (EntityData JSON)
- **Caching**: None in this application

### Transliteration API

- **State location**: External service (nucilandia.ro)
- **Data**: Transliteration rules/models per language
- **Consistency**: Versioned deployment
- **Access**: Read-only via HTTP GET
- **Caching**: None in this application

## Persistence

### Application-Level Persistence

**None**. The application does not use:
- Databases (SQL, NoSQL)
- File-based storage (except logs)
- In-memory caches (Redis, MemoryCache)
- Session state
- Distributed caches

### Log Persistence

**File**: `logfile.log` (configurable via `NuciLoggerSettings.LogFilePath`)

- **Format**: Structured text (NuciLog format)
- **Rotation**: Not configured (single file, grows indefinitely)
- **Retention**: Not configured (manual cleanup required)
- **Content**: Request/response logs, operation traces, errors

**Configuration**:
```json
{
  "nuciLoggerSettings": {
    "logFilePath": "logfile.log",
    "isFileOutputEnabled": true
  }
}
```

## Caching Strategy

### Current: No Caching

Every request:
1. Calls GeoNames (if `geoNamesId` provided)
2. Calls WikiData (if `wikiDataId` provided)
3. Calls Transliteration API for each non-Latin name
4. Processes and returns result

### Potential Caching Opportunities (Not Implemented)

| Cache Level | Key | TTL | Invalidation |
|-------------|-----|-----|--------------|
| GeoNames response | `geonames:{id}` | 24h | Manual / cache expiry |
| WikiData response | `wikidata:{id}` | 24h | Manual / cache expiry |
| Transliteration | `translit:{lang}:{text}` | 30d | Manual / cache expiry |
| Full response | `exonyms:{geoNamesId}:{wikiDataId}` | 1h | Manual / cache expiry |

**Rationale for no caching**: Low expected traffic, data freshness priority, simplicity.

## Migration Considerations

### Adding Persistence (Future)

If persistence is added later:

1. **Database**: Add `DbContext`, migrations, connection string config
2. **Caching**: Add `IMemoryCache` or `IDistributedCache`, decorate gatherers
3. **Schema**: `Location` → table with `DefaultName`, `Names` as JSON or separate table
4. **Invalidation**: Time-based or event-driven (webhook from WikiData/GeoNames if available)

### Configuration Migration

- Settings classes are POCOs; adding properties is backward compatible
- `appsettings.json` schema changes require deployment coordination
- Environment variable naming follows ASP.NET Core conventions (`Section__Property`)

## Concurrency Model

- **Request handling**: Thread-per-request (Kestrel default)
- **Async/await**: Used throughout for I/O (HTTP, logging)
- **No shared mutable state**: No locks, no concurrent collections needed
- **HttpClient**: Created per-call in gatherers (short-lived), or via `HttpClientCreator` (pooled)

## Failure State

### Transient Failures

| Failure | Behaviour | Recovery |
|---------|-----------|----------|
| GeoNames HTTP error | Exception thrown, logged, propagates to controller | Retry by client |
| WikiData HTTP error | Exception thrown, logged, propagates to controller | Retry by client |
| Transliteration API error | Returns original name (graceful degradation) | N/A (best effort) |
| Transliteration timeout | Returns original name | N/A |

### Permanent Failures

| Failure | Behaviour |
|---------|-----------|
| Missing HMAC key | `SignHMAC` throws / produces invalid signature |
| Invalid transliteration URL | `NuciApiClient` throws on first use |
| Malformed external response | Parsing exception, logged, propagates |

No circuit breaker, retry policy, or fallback cache implemented.

## Scaling Considerations

### Horizontal Scaling

- **Stateless**: Multiple instances can run behind load balancer
- **No sticky sessions required**
- **Log aggregation**: Required (file logs not shared)

### Resource Usage

- **Memory**: Low (no caching, small object graphs)
- **CPU**: Moderate (regex normalisation, transliteration calls)
- **Network**: 2-3 external HTTP calls per request
- **Thread pool**: Standard ASP.NET Core scaling

### Bottlenecks

1. **External API latency** (GeoNames, WikiData, Transliteration)
2. **GeoNames username rotation** (hardcoded list, no dynamic discovery)
3. **Regex normalisation** (hundreds of patterns per name)
4. **Sequential transliteration calls** (one per non-Latin name)

### Optimisation Opportunities

- Parallel transliteration calls (currently sequential in gatherers)
- HttpClient pooling (currently per-call in GeoNamesGatherer)
- Response caching (if traffic increases)
- Pre-computed normalisation patterns (compiled regex)