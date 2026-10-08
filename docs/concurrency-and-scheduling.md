# Concurrency and Scheduling

## Overview

The Exonyms API is designed as a stateless, on-demand processing service. Concurrency is handled at the HTTP request level by ASP.NET Core's Kestrel server. There is no background task scheduling, no persistent state, and no long-running operations.

## Threading Model

### ASP.NET Core Threading

```
Kestrel Server
    │
    ├── Thread Pool (default)
    │   ├── Request threads (per HTTP request)
    │   └── Background threads (framework tasks)
    │
    └── Request Processing
        ├── Middleware pipeline (synchronous per request)
        ├── Controller action (synchronous, blocking .Result)
        └── Service layer (async/await)
```

### Service Lifetimes

| Service | Lifetime | Thread Safety |
|---------|----------|---------------|
| `ExonymsService` | Singleton | Not thread-safe (per-request state) |
| `NameNormaliser` | Singleton | Thread-safe (stateless) |
| `NameConstructor` | Singleton | Thread-safe (stateless) |
| `GeoNamesGatherer` | Singleton | Not thread-safe (HttpClient per call) |
| `WikiDataGatherer` | Singleton | Not thread-safe (HttpClient per call) |
| `TransliterationApiClient` | Transient | Thread-safe (stateless) |
| `NuciApiClient` | Transient | Thread-safe (stateless) |
| `NuciLogger` | Transient | Thread-safe (stateless) |

### Thread Safety Analysis

#### Thread-Safe Components

**NameNormaliser**:
- Stateless: only contains regex patterns
- All methods are pure functions
- Safe for concurrent access

**NameConstructor**:
- Stateless: only contains transformation rules
- All methods are pure functions
- Safe for concurrent access

**TransliterationApiClient**:
- Transient: new instance per request
- No shared mutable state
- Safe for concurrent access

#### Potentially Unsafe Components

**ExonymsService**:
- Singleton: shared across all requests
- `Gather()` creates new `Location` objects per call
- No shared mutable state between calls
- **Safe in practice**: each call is independent

**GeoNamesGatherer**:
- Singleton: shared across all requests
- Creates new `HttpClient` per `Gather()` call
- No shared mutable state between calls
- **Safe in practice**: each call is independent

**WikiDataGatherer**:
- Singleton: shared across all requests
- Creates new `HttpClient` per `Gather()` call
- No shared mutable state between calls
- **Safe in practice**: each call is independent

## Concurrency Patterns

### Request-Level Concurrency

```
Multiple HTTP Requests
    │
    ├── Request 1 → Thread 1 → ExonymsService.Gather() → Location1
    ├── Request 2 → Thread 2 → ExonymsService.Gather() → Location2
    ├── Request 3 → Thread 3 → ExonymsService.Gather() → Location3
    └── ...
```

Each request:
1. Creates its own `Location` object
2. Calls gatherers (which create their own `HttpClient`)
3. Processes names independently
4. Returns response

**No shared mutable state** between concurrent requests.

### HttpClient Usage

**Current Implementation**:
```csharp
// In GeoNamesGatherer
private readonly HttpClient httpClient = new HttpClient();

// In WikiDataGatherer
private readonly HttpClient httpClient = new HttpClient();
```

**Issues**:
- `HttpClient` instances are not disposed (memory leak over time)
- No connection pooling
- No retry policies
- No circuit breaker

**Recommendation**: Use `IHttpClientFactory` for proper lifecycle management.

### Blocking Calls

**Current Implementation**:
```csharp
// In ExonymsController
Location exonyms = exonymsService.Gather(request.GeoNamesId, request.WikiDataId).Result;
```

**Issues**:
- `.Result` blocks the request thread
- Can cause thread pool starvation under load
- Potential deadlock in certain contexts

**Recommendation**: Use `await` instead of `.Result`:
```csharp
Location exonyms = await exonymsService.Gather(request.GeoNamesId, request.WikiDataId);
```

## Scheduling

### Background Tasks

**None implemented**. The API is purely request-driven.

### Timers

**None implemented**. No scheduled tasks.

### Cron Jobs

**None implemented**. No periodic processing.

## Asynchronous Processing

### Current Async Usage

```csharp
// ExonymsService.Gather()
public async Task<Location> Gather(int? geoNamesId, string wikiDataId)
{
    Location wikiDataLocation = await wikiDataGatherer.Gather(wikiDataId);
    Location geoNamesLocation = await geoNamesGatherer.Gather(geoNamesId);
    // ...
}

// Gatherers
public async Task<Location> Gather(string wikiDataId)
{
    string json = await httpClient.GetStringAsync(url);
    // ...
}
```

### Async/Await Chain

```
Controller.Get()
    │
    ├── .Result (blocking)
    │
    ▼
ExonymsService.Gather()
    │
    ├── await WikiDataGatherer.Gather()
    │   ├── await HttpClient.GetStringAsync()
    │   ├── await TransliterationApiClient.Transliterate() (per name)
    │   └── await NameNormaliser.Normalise() (per name)
    │
    ├── await GeoNamesGatherer.Gather()
    │   ├── await HttpClient.GetStringAsync()
    │   ├── await TransliterationApiClient.Transliterate() (per name)
    │   └── await NameNormaliser.Normalise() (per name)
    │
    ├── await TransliterationApiClient.Transliterate() (fallbacks)
    │
    └── await NameConstructor.Construct() (construction)
```

### Parallel Processing Opportunities

**Current**: Sequential processing of WikiData → GeoNames

**Opportunity**: Parallel execution:
```csharp
var wikiDataTask = wikiDataGatherer.Gather(wikiDataId);
var geoNamesTask = geoNamesGatherer.Gather(geoNamesId);
await Task.WhenAll(wikiDataTask, geoNamesTask);
```

**Benefit**: Reduces total latency by ~50% (parallel external calls)

## Resource Contention

### External API Limits

| Service | Limit | Contention Risk |
|---------|-------|-----------------|
| GeoNames | ~2000 req/hour per username | Medium (60+ usernames) |
| WikiData | No strict limit | Low |
| Transliteration | Unknown | Low |

### Connection Pooling

**Current**: No connection pooling (new `HttpClient` per gatherer)

**Recommendation**: Use `IHttpClientFactory` with named/typed clients.

## Scalability

### Horizontal Scaling

**Stateless**: Can be scaled horizontally behind a load balancer.

**Requirements**:
- Shared configuration (environment variables)
- Shared HMAC key (for response verification)
- No sticky sessions needed

### Vertical Scaling

**Thread Pool**: Default ASP.NET Core thread pool settings.

**Kestrel**: Default connection limits.

### Load Testing Considerations

**Bottlenecks**:
1. External API calls (WikiData, GeoNames, Transliteration)
2. Blocking `.Result` calls
3. `HttpClient` creation per request

**Metrics to Monitor**:
- Request latency
- Thread pool usage
- External API response times
- Error rates

## Concurrency Testing

### Current Tests

**None**: No concurrency tests in the test suite.

### Recommended Tests

1. **Concurrent request handling**:
   - Multiple simultaneous requests
   - Verify no data corruption
   - Verify correct responses

2. **Thread safety verification**:
   - Concurrent calls to `ExonymsService.Gather()`
   - Verify independent `Location` objects

3. **HttpClient lifecycle**:
   - Verify proper disposal
   - Verify connection reuse

### Test Example

```csharp
[Test]
public async Task Gather_ConcurrentRequests_ReturnsCorrectResults()
{
    var tasks = Enumerable.Range(0, 100)
        .Select(_ => service.Gather(310350, "Q310350"))
        .ToArray();

    var results = await Task.WhenAll(tasks);

    Assert.That(results, Has.Length.EqualTo(100));
    Assert.That(results.All(r => r.DefaultName == "Al Hoceima"));
}
```

## Deadlock Prevention

### Current Risk

```csharp
// Controller uses .Result which can deadlock
Location exonyms = exonymsService.Gather(...).Result;
```

**Risk**: In ASP.NET Core, `.Result` can cause deadlocks if the async method doesn't complete synchronously.

**Mitigation**: The current code works because:
- `HttpClient.GetStringAsync()` completes asynchronously
- The thread pool can handle the blocking
- No `SynchronizationContext` in ASP.NET Core (unlike ASP.NET)

**Recommendation**: Use `await` to eliminate risk entirely.

## Memory Management

### Object Creation Per Request

| Object | Created Per Request | Notes |
|--------|---------------------|-------|
| `Location` | Yes | New instance per `Gather()` call |
| `Name` | Yes (per language) | New instance per name |
| `HttpClient` | Yes (per gatherer) | Should be pooled |
| `JObject`/`XDocument` | Yes | Parsed per request |
| `Regex` | No | Static/shared in normaliser |

### Garbage Collection

**Pressure Points**:
- `HttpClient` instances (not disposed)
- Large JSON/XML documents
- Regex match objects

**Recommendation**:
- Use `IHttpClientFactory`
- Consider streaming JSON parsing for large responses
- Reuse `Regex` instances (already done via static patterns)

## Performance Characteristics

### Concurrency Limits

| Component | Limit | Notes |
|-----------|-------|-------|
| Kestrel connections | ~10000 | Default |
| Thread pool threads | ~25-100 | CPU-dependent |
| External API calls | Variable | Service-dependent |

### Throughput

**Estimated**: 10-50 requests/second (limited by external API latency)

**Bottlenecks**:
1. External API calls (300-800ms per request)
2. Blocking `.Result` calls
3. `HttpClient` creation overhead

## Recommendations

### Immediate

1. **Replace `.Result` with `await`** in controller
2. **Use `IHttpClientFactory`** for HTTP client management
3. **Add parallel execution** for WikiData/GeoNames gathering

### Short-term

1. **Add concurrency tests** to verify thread safety
2. **Implement connection pooling** for external APIs
3. **Add circuit breaker** for external service failures

### Long-term

1. **Add caching layer** to reduce external API calls
2. **Implement rate limiting** for external services
3. **Add health checks** for external service monitoring
4. **Consider background caching** of popular locations

## Related Documentation

- [Architecture](../architecture.md)
- [Components: Application Services](../components/application-services.md)
- [Error Handling](../error-handling.md)
- [Build and Deployment](../build-and-deployment.md)
- [Testing](../testing.md)