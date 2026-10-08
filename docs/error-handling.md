# Error Handling

## Overview

The Exonyms API implements a layered error handling strategy using the NuciAPI middleware pipeline. Errors are caught at the middleware level, logged, and returned as standardized JSON responses.

## Error Handling Architecture

```
┌─────────────────────────────────────────────────────────────┐
│                    Error Handling Layers                       │
├─────────────────────────────────────────────────────────────┤
│                                                             │
│  Layer 1: External API Errors                                │
│  ┌─────────────────────────────────────────────────────┐    │
│  │ WikiDataGatherer / GeoNamesGatherer                  │    │
│  │ - HttpRequestException (HTTP errors)                 │    │
│  │ - JsonReaderException (JSON parse errors)            │    │
│  │ - XmlException (XML parse errors)                    │    │
│  └─────────────────────────────────────────────────────┘    │
│                          │                                    │
│                          ▼                                    │
│  Layer 2: Transliteration Errors                             │
│  ┌─────────────────────────────────────────────────────┐    │
│  │ TransliterationApiClient                             │    │
│  │ - Graceful degradation (return original)             │    │
│  │ - No exception thrown                                │    │
│  └─────────────────────────────────────────────────────┘    │
│                          │                                    │
│                          ▼                                    │
│  Layer 3: Service Layer Errors                               │
│  ┌─────────────────────────────────────────────────────┐    │
│  │ ExonymsService.Gather()                              │    │
│  │ - Propagates exceptions from gatherers               │    │
│  │ - No internal error handling                         │    │
│  └─────────────────────────────────────────────────────┘    │
│                          │                                    │
│                          ▼                                    │
│  Layer 4: Middleware Pipeline                                │
│  ┌─────────────────────────────────────────────────────┐    │
│  │ NuciApiExceptionHandling                             │    │
│  │ - Catches all unhandled exceptions                   │    │
│  │ - Logs error with correlation ID                     │    │
│  │ - Returns standardized error response                │    │
│  └─────────────────────────────────────────────────────┘    │
│                          │                                    │
│                          ▼                                    │
│  Layer 5: HTTP Response                                      │
│  ┌─────────────────────────────────────────────────────┐    │
│  │ NuciApiErrorResponse                                 │    │
│  │ - success: false                                     │    │
│  │ - error: { code, message }                           │    │
│  │ - requestId: correlation ID                          │    │
│  └─────────────────────────────────────────────────────┘    │
│                                                             │
└─────────────────────────────────────────────────────────────┘
```

## Error Types

### 1. External API Errors

#### WikiData Errors

| Error Type | Exception | Handling | HTTP Status |
|------------|-----------|----------|-------------|
| HTTP 4xx/5xx | `HttpRequestException` | Propagate | 500 |
| JSON parse error | `JsonReaderException` | Propagate | 500 |
| Missing entity | Returns empty Location | Silent | 200 |
| Missing labels | Empty Names | Silent | 200 |

**Example**:
```csharp
// WikiDataGatherer.Gather()
string json = await httpClient.GetStringAsync(url);
JObject data = JObject.Parse(json);
JObject entity = (JObject)data["entities"][wikiDataId];
// If entity is null → NullReferenceException → 500
```

#### GeoNames Errors

| Error Type | Exception | Handling | HTTP Status |
|------------|-----------|----------|-------------|
| HTTP 4xx/5xx | `HttpRequestException` | Propagate | 500 |
| XML parse error | `XmlException` | Propagate | 500 |
| Missing `<name>` | Null reference | Propagate | 500 |

**Example**:
```csharp
// GeoNamesGatherer.Gather()
string xml = await httpClient.GetStringAsync(url);
XDocument doc = XDocument.Parse(xml);
XElement geoname = doc.Root;
string name = (string)geoname.Element("name");
// If geoname is null → NullReferenceException → 500
```

#### Transliteration Errors

| Error Type | Exception | Handling | HTTP Status |
|------------|-----------|----------|-------------|
| Language not supported | None | Return original | 200 |
| API HTTP error | None | Return original | 200 |
| API unsuccessful envelope | None | Return original | 200 |
| Empty response.Text | None | Return original | 200 |

**Example**:
```csharp
// TransliterationApiClient.Transliterate()
try
{
    // Call external API
    // If error → return original text
}
catch
{
    return originalText; // Graceful degradation
}
```

### 2. Service Layer Errors

#### ExonymsService Errors

| Error Type | Exception | Handling | HTTP Status |
|------------|-----------|----------|-------------|
| WikiData failure | `HttpRequestException` | Propagate | 500 |
| GeoNames failure | `HttpRequestException` | Propagate | 500 |
| Transliteration failure | None | Return original | 200 |
| Normalisation failure | `Regex` error | Propagate | 500 |
| Construction failure | `Regex` error | Propagate | 500 |

**Example**:
```csharp
// ExonymsService.Gather()
Location wikiDataLocation = await wikiDataGatherer.Gather(wikiDataId);
Location geoNamesLocation = await geoNamesGatherer.Gather(geoNamesId);
// If either throws → exception propagates to controller
```

### 3. Middleware Errors

#### NuciApiExceptionHandling

**Catches**: All unhandled exceptions in the middleware pipeline.

**Behaviour**:
1. Log error with correlation ID
2. Create `NuciApiErrorResponse`
3. Set HTTP status code (500 for unhandled)
4. Return JSON error response

**Example**:
```csharp
// In middleware
catch (Exception ex)
{
    logger.Error(Operation.HttpRequest, ex, "Request failed");
    return new NuciApiErrorResponse
    {
        Success = false,
        Error = new NuciApiError
        {
            Code = "InternalServerError",
            Message = ex.Message
        },
        RequestId = correlationId
    };
}
```

### 4. Validation Errors

#### Request Validation

| Error Type | Exception | Handling | HTTP Status |
|------------|-----------|----------|-------------|
| Missing parameters | None | Return 400 | 400 |
| Invalid parameter type | `FormatException` | Return 400 | 400 |

**Example**:
```csharp
// In controller
if (request.GeoNamesId == null && string.IsNullOrEmpty(request.WikiDataId))
{
    return BadRequest("At least one of geoNamesId or wikiDataId must be provided.");
}
```

## Error Response Format

### Standard Error Response

```json
{
  "success": false,
  "error": {
    "code": "InternalServerError",
    "message": "Failed to retrieve the WikiData entry for 'Q123': NotFound"
  },
  "requestId": "a1b2c3d4-e5f6-7890-abcd-ef1234567890"
}
```

### Error Fields

| Field | Type | Description |
|-------|------|-------------|
| `success` | `boolean` | Always `false` for errors |
| `error.code` | `string` | Error code (e.g., `InternalServerError`, `BadRequest`) |
| `error.message` | `string` | Human-readable error message |
| `requestId` | `string` | Correlation ID for tracing |

### HTTP Status Codes

| Status Code | Error Type | Example |
|-------------|------------|---------|
| 400 | Bad Request | Missing parameters |
| 500 | Internal Server Error | External API failure |
| 503 | Service Unavailable | External service down |

## Logging

### Error Logging

**Operation**: `MyOperation.HttpRequest`
**Status**: `Failure`
**LogInfo**:
- `RequestId`: Correlation ID
- `Exception`: Exception details
- `RequestPath`: Request path
- `QueryString`: Query parameters

**Example**:
```
Operation: HttpRequest
Status: Failure
LogInfo:
  - RequestId: a1b2c3d4-e5f6-7890-abcd-ef1234567890
  - Exception: HttpRequestException: NotFound
  - RequestPath: /Exonyms
  - QueryString: geoNamesId=310350&wikiDataId=Q310350
```

### Error Context

**Correlation ID**: Generated per request, included in all logs and responses.

**Request Context**:
- Method (GET)
- Path (/Exonyms)
- Query parameters
- Headers

## Error Handling in Components

### GeoNamesGatherer

```csharp
public async Task<Location> Gather(int geoNamesId)
{
    try
    {
        string username = usernames.GetRandomElement();
        string url = $"http://api.geonames.org/get?geonameId={geoNamesId}&username={username}";
        string xml = await httpClient.GetStringAsync(url);
        XDocument doc = XDocument.Parse(xml);
        // ...
    }
    catch (Exception ex)
    {
        logger.Error(MyOperation.Gather, ex, "Failed to retrieve the GeoNames entry for '{GeoNamesId}'", geoNamesId);
        throw; // Propagate to service layer
    }
}
```

### WikiDataGatherer

```csharp
public async Task<Location> Gather(string wikiDataId)
{
    try
    {
        string url = $"https://wikidata.org/wiki/Special:EntityData/{wikiDataId}.json";
        string json = await httpClient.GetStringAsync(url);
        JObject data = JObject.Parse(json);
        // ...
    }
    catch (Exception ex)
    {
        logger.Error(MyOperation.Gather, ex, "Failed to retrieve the WikiData entry for '{WikiDataId}'", wikiDataId);
        throw; // Propagate to service layer
    }
}
```

### TransliterationApiClient

```csharp
public async Task<string> Transliterate(string languageCode, string text)
{
    try
    {
        // Call external API
        // If error → return original text
    }
    catch
    {
        return text; // Graceful degradation
    }
}
```

## Retry Strategy

### Current State

**No retry logic** implemented for external API calls.

### Recommended Retry Strategy

```csharp
// Example: Retry with exponential backoff
public async Task<Location> GatherWithRetry(string wikiDataId, int maxRetries = 3)
{
    for (int attempt = 1; attempt <= maxRetries; attempt++)
    {
        try
        {
            return await wikiDataGatherer.Gather(wikiDataId);
        }
        catch (HttpRequestException ex) when (attempt < maxRetries)
        {
            logger.Warning(MyOperation.Gather, ex, "Attempt {Attempt} failed, retrying...", attempt);
            await Task.Delay(TimeSpan.FromSeconds(Math.Pow(2, attempt)));
        }
    }
    throw new Exception("All retry attempts failed");
}
```

## Circuit Breaker Pattern

### Current State

**Not implemented**. External API failures result in immediate 500 errors.

### Recommended Implementation

```csharp
// Example: Circuit breaker for external APIs
public class CircuitBreaker
{
    private int _failureCount = 0;
    private DateTime _lastFailureTime;
    private readonly int _threshold = 5;
    private readonly TimeSpan _timeout = TimeSpan.FromMinutes(1);

    public bool IsOpen => _failureCount >= _threshold && DateTime.UtcNow - _lastFailureTime < _timeout;

    public void RecordSuccess() => _failureCount = 0;

    public void RecordFailure()
    {
        _failureCount++;
        _lastFailureTime = DateTime.UtcNow;
    }
}
```

## Error Handling Best Practices

### Do

- Log all errors with correlation ID
- Return standardized error responses
- Use appropriate HTTP status codes
- Implement graceful degradation for external failures
- Include error context in logs

### Don't

- Don't expose internal details in error messages
- Don't swallow exceptions silently
- Don't retry indefinitely
- Don't leak stack traces to clients

## Testing Error Handling

### Unit Tests

- Test error scenarios for each component
- Verify error logging
- Verify error response format

### Integration Tests

- Test external API failures
- Test retry logic
- Test circuit breaker behaviour

## Related Documentation

- [Architecture](../architecture.md)
- [Components: Application Services](../components/application-services.md)
- [Logging](../logging.md)
- [Testing](../testing.md)
- [Build and Deployment](../build-and-deployment.md)