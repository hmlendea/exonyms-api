# Logging

## Overview

The Exonyms API uses **NuciLog** for structured logging with custom operation types and log info keys. All logging is done through the `ILogger` interface injected via DI.

## Logging Framework

| Component | Version | Purpose |
|-----------|---------|---------|
| NuciLog | 1.1.2 | Structured logging |
| NuciLog.Core | 2.5.0 | Core logging infrastructure |

## Log Structure

### Operation Types

**File**: `ExonymsAPI/Logging/MyOperation.cs`

```csharp
public static class MyOperation
{
    public const string HttpRequest = "HttpRequest";
    public const string Gather = "Gather";
    public const string Transliterate = "Transliterate";
    public const string Normalise = "Normalise";
    public const string Construct = "Construct";
}
```

### Log Info Keys

**File**: `ExonymsAPI/Logging/MyLogInfoKey.cs`

```csharp
public static class MyLogInfoKey
{
    public const string RequestId = "RequestId";
    public const string GeoNamesId = "GeoNamesId";
    public const string WikiDataId = "WikiDataId";
    public const string LanguageCode = "LanguageCode";
    public const string OriginalValue = "OriginalValue";
    public const string TransliteratedValue = "TransliteratedValue";
    public const string NormalisedValue = "NormalisedValue";
    public const string ConstructedValue = "ConstructedValue";
    public const string FallbackLanguage = "FallbackLanguage";
    public const string Exception = "Exception";
    public const string RequestPath = "RequestPath";
    public const string QueryString = "QueryString";
    public const string StatusCode = "StatusCode";
    public const string DurationMs = "DurationMs";
}
```

## Log Levels

| Level | Usage |
|-------|-------|
| `Trace` | Detailed flow information |
| `Debug` | Debugging information |
| `Information` | General operational information |
| `Warning` | Potential issues, degraded performance |
| `Error` | Handled errors, external API failures |
| `Critical` | Unhandled exceptions, system failures |

## Logging in Components

### Middleware Logging (NuciApiRequestLogging)

**Automatic**: All HTTP requests/responses logged.

**Request Log**:
```
Operation: HttpRequest
Status: Started
LogInfo:
  - RequestId: a1b2c3d4-e5f6-7890-abcd-ef1234567890
  - Method: GET
  - Path: /Exonyms
  - Query: geoNamesId=310350&wikiDataId=Q310350
  - Headers: { ... }
```

**Response Log**:
```
Operation: HttpRequest
Status: Success
LogInfo:
  - RequestId: a1b2c3d4-e5f6-7890-abcd-ef1234567890
  - StatusCode: 200
  - DurationMs: 456
  - ResponseBody: { "success": true, "defaultName": "..." }
```

### Gatherer Logging

#### WikiDataGatherer

```csharp
// Start
logger.Info(MyOperation.Gather, "Retrieving WikiData entry for '{WikiDataId}'", wikiDataId);

// Success
logger.Info(MyOperation.Gather, "Retrieved WikiData entry for '{WikiDataId}' with {Count} names", wikiDataId, location.Names.Count);

// Error
logger.Error(MyOperation.Gather, ex, "Failed to retrieve the WikiData entry for '{WikiDataId}'", wikiDataId);
```

#### GeoNamesGatherer

```csharp
// Start
logger.Info(MyOperation.Gather, "Retrieving GeoNames entry for '{GeoNamesId}'", geoNamesId);

// Success
logger.Info(MyOperation.Gather, "Retrieved GeoNames entry for '{GeoNamesId}' with {Count} names", geoNamesId, location.Names.Count);

// Error
logger.Error(MyOperation.Gather, ex, "Failed to retrieve the GeoNames entry for '{GeoNamesId}'", geoNamesId);
```

### Transliteration Logging

```csharp
// TransliterationApiClient
logger.Info(MyOperation.Transliterate, "Transliterating '{OriginalValue}' from '{LanguageCode}'", originalValue, languageCode);

// Success
logger.Info(MyOperation.Transliterate, "Transliterated '{OriginalValue}' to '{TransliteratedValue}'", originalValue, transliteratedValue);

// Error (graceful degradation)
logger.Warning(MyOperation.Transliterate, ex, "Transliteration failed for '{OriginalValue}', returning original", originalValue);
```

### Normalisation Logging

```csharp
// NameNormaliser
logger.Info(MyOperation.Normalise, "Normalising '{OriginalValue}' for language '{LanguageCode}'", originalValue, languageCode);

// Success
logger.Info(MyOperation.Normalise, "Normalised '{OriginalValue}' to '{NormalisedValue}'", originalValue, normalisedValue);
```

### Construction Logging

```csharp
// NameConstructor
logger.Info(MyOperation.Construct, "Constructing '{OriginalValue}' for language '{LanguageCode}'", originalValue, languageCode);

// Success
logger.Info(MyOperation.Construct, "Constructed '{OriginalValue}' to '{ConstructedValue}'", originalValue, constructedValue);
```

### Service Layer Logging

```csharp
// ExonymsService
logger.Info(MyOperation.Gather, "Gathering exonyms for GeoNamesId={GeoNamesId}, WikiDataId={WikiDataId}", geoNamesId, wikiDataId);

// Merge
logger.Info(MyOperation.Gather, "Merged WikiData ({WikiDataCount}) and GeoNames ({GeoNamesCount}) names", wikiDataCount, geoNamesCount);

// Construction
logger.Info(MyOperation.Gather, "Constructed {Count} names", constructedCount);

// Fallbacks
logger.Info(MyOperation.Gather, "Applied fallbacks for {Count} languages", fallbackCount);

// Deduplication
logger.Info(MyOperation.Gather, "Removed {Count} redundant exonyms", removedCount);

// Sorting
logger.Info(MyOperation.Gather, "Sorted {Count} names alphabetically", sortedCount);
```

## Log Output Format

### Console Output (Development)

```
[2026-10-08 12:00:00.123] [INFO] [HttpRequest] Started - RequestId=a1b2c3d4-e5f6-7890-abcd-ef1234567890 Method=GET Path=/Exonyms Query=geoNamesId=310350&wikiDataId=Q310350
[2026-10-08 12:00:00.124] [INFO] [Gather] Retrieving WikiData entry for 'Q310350'
[2026-10-08 12:00:00.345] [INFO] [Gather] Retrieved WikiData entry for 'Q310350' with 12 names
[2026-10-08 12:00:00.346] [INFO] [Gather] Retrieving GeoNames entry for '310350'
[2026-10-08 12:00:00.567] [INFO] [Gather] Retrieved GeoNames entry for '310350' with 8 names
[2026-10-08 12:00:00.568] [INFO] [Gather] Merged WikiData (12) and GeoNames (8) names
[2026-10-08 12:00:00.569] [INFO] [Transliterate] Transliterating 'Москва' from 'ru'
[2026-10-08 12:00:00.570] [INFO] [Transliterate] Transliterated 'Москва' to 'Moskva'
[2026-10-08 12:00:00.571] [INFO] [Normalise] Normalising 'Moskva' for language 'ru'
[2026-10-08 12:00:00.572] [INFO] [Normalise] Normalised 'Moskva' to 'Moskva'
[2026-10-08 12:00:00.573] [INFO] [Gather] Constructed 1 names
[2026-10-08 12:00:00.574] [INFO] [Gather] Applied fallbacks for 3 languages
[2026-10-08 12:00:00.575] [INFO] [Gather] Removed 2 redundant exonyms
[2026-10-08 12:00:00.576] [INFO] [Gather] Sorted 15 names alphabetically
[2026-10-08 12:00:00.577] [INFO] [HttpRequest] Success - RequestId=a1b2c3d4-e5f6-7890-abcd-ef1234567890 StatusCode=200 DurationMs=454
```

### File Output (Production)

**Configuration**: `NuciLoggerSettings.IsFileOutputEnabled` + `LogFilePath`

**Format**: JSON lines (one JSON object per line)

```json
{
  "timestamp": "2026-10-08T12:00:00.123Z",
  "level": "Information",
  "operation": "HttpRequest",
  "status": "Started",
  "logInfo": {
    "RequestId": "a1b2c3d4-e5f6-7890-abcd-ef1234567890",
    "Method": "GET",
    "Path": "/Exonyms",
    "Query": "geoNamesId=310350&wikiDataId=Q310350"
  }
}
```

## Configuration

### NuciLoggerSettings

**File**: `appsettings.json`

```json
{
  "NuciLoggerSettings": {
    "IsFileOutputEnabled": true,
    "LogFilePath": "logs/exonyms-api.log",
    "MinimumLevel": "Information"
  }
}
```

### Environment-Specific

**Development** (`appsettings.Development.json`):
```json
{
  "NuciLoggerSettings": {
    "IsFileOutputEnabled": false,
    "MinimumLevel": "Debug"
  }
}
```

**Production** (`appsettings.Production.json`):
```json
{
  "NuciLoggerSettings": {
    "IsFileOutputEnabled": true,
    "LogFilePath": "/var/log/exonyms-api/exonyms-api.log",
    "MinimumLevel": "Information"
  }
}
```

## Correlation IDs

### Generation

- Generated by `NuciApiRequestLogging` middleware
- Stored in `HttpContext.Items["RequestId"]`
- Propagated to all downstream loggers

### Usage

```csharp
// In services
string requestId = httpContextAccessor.HttpContext?.Items["RequestId"]?.ToString();
logger.Info(MyOperation.Gather, "Processing request", requestId);
```

### Response Header

- Added to response: `X-Request-Id: a1b2c3d4-e5f6-7890-abcd-ef1234567890`
- Included in error responses: `requestId` field

## Structured Logging Benefits

### Queryable Logs

```sql
-- Find all requests for a specific GeoNamesId
SELECT * FROM logs WHERE logInfo.GeoNamesId = '310350'

-- Find all transliteration failures
SELECT * FROM logs WHERE operation = 'Transliterate' AND level = 'Warning'

-- Find slow requests
SELECT * FROM logs WHERE operation = 'HttpRequest' AND logInfo.DurationMs > 1000
```

### Alerting

```yaml
# Example alert rules
- alert: HighErrorRate
  expr: rate(logs{level="Error"}[5m]) > 0.1

- alert: SlowRequests
  expr: histogram_quantile(0.95, rate(logs{operation="HttpRequest", status="Success"}[5m])) > 2000
```

## Sensitive Data Handling

### Masking

**NuciLog** supports automatic masking of sensitive fields:

```csharp
// Configuration
logger.Mask("Authorization", "Cookie", "X-Api-Key");
```

### Current Implementation

- No explicit masking configured
- Headers logged in request/response
- **Recommendation**: Add masking for sensitive headers

## Log Retention

### Development

- Console only
- No persistence

### Production

- File rotation (daily)
- Retention: 30 days (configurable)
- Compression: gzip

## Performance

### Async Logging

- NuciLog uses async sinks
- Non-blocking for request processing
- Buffered writes

### Overhead

| Operation | Log Calls | Estimated Overhead |
|-----------|-----------|-------------------|
| Single request | ~20-30 | < 1ms |
| High throughput | 1000 req/s | ~5-10ms CPU |

## Monitoring Integration

### Log Aggregation

**Recommended**: ELK Stack (Elasticsearch, Logstash, Kibana) or similar

**Pipeline**:
```
Application → Filebeat → Logstash → Elasticsearch → Kibana
```

### Metrics Extraction

```logstash
# Extract duration as metric
filter {
  if [operation] == "HttpRequest" and [status] == "Success" {
    mutate { add_field => { "duration_ms" => "%{[logInfo][DurationMs]}" } }
  }
}
```

## Debugging with Logs

### Common Queries

```bash
# Find request by ID
grep "a1b2c3d4-e5f6-7890-abcd-ef1234567890" logs/exonyms-api.log

# Find all errors
grep '"level":"Error"' logs/exonyms-api.log

# Find slow requests
jq 'select(.logInfo.DurationMs > 1000)' logs/exonyms-api.log

# Find transliteration failures
jq 'select(.operation=="Transliterate" and .level=="Warning")' logs/exonyms-api.log
```

## Testing Logging

### Unit Tests

```csharp
[Test]
public void Gather_LogsCorrectOperations()
{
    var logger = new Mock<ILogger>();
    var service = new ExonymsService(..., logger.Object);

    service.Gather(310350, "Q310350");

    logger.Verify(x => x.Info(MyOperation.Gather, It.IsAny<string>(), It.IsAny<object[]>()), Times.AtLeastOnce);
}
```

### Integration Tests

- Verify log output format
- Verify correlation ID propagation
- Verify sensitive data masking

## Related Documentation

- [Architecture](../architecture.md)
- [Components: Application Services](../components/application-services.md)
- [Error Handling](../error-handling.md)
- [Configuration](../configuration.md)
- [Security](../security.md)