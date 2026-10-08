# Presentation Layer

## ExonymsController

**File**: `ExonymsAPI/API/Controllers/ExonymsController.cs`
**Namespace**: `ExonymsAPI.API.Controllers`
**Base Class**: `NuciApiController` (from `NuciAPI.Controllers`)

### Endpoint

```
GET /Exonyms
```

### Action Method

```csharp
[HttpGet]
public ActionResult Get([FromQuery] GetExonymsRequest request)
    => ProcessRequest(
        request,
        () =>
        {
            Location exonyms = exonymsService.Gather(request.GeoNamesId, request.WikiDataId).Result;

            GetExonymsResponse response = new()
            {
                DefaultName = exonyms.DefaultName,
                Names = exonyms.Names
            };

            response.SignHMAC(securitySettings.HmacSigningKey);

            return response;
        },
        NuciApiAuthorisation.None);
```

### Parameters

| Parameter | Source | Type | Required | Description |
|-----------|--------|------|----------|-------------|
| `geoNamesId` | Query | `string` | No | GeoNames feature ID (e.g., `310350`) |
| `wikiDataId` | Query | `string` | No | WikiData entity ID (e.g., `Q310350`) |

At least one ID should be provided; both empty returns empty `Location`.

### Dependencies (Injected)

| Dependency | Interface | Lifetime |
|------------|-----------|----------|
| `exonymsService` | `IExonymsService` | Singleton |
| `securitySettings` | `SecuritySettings` | Singleton |

### Behaviour

1. **Request validation**: `ProcessRequest` handles model binding, validation
2. **Service call**: `exonymsService.Gather(geoNamesId, wikiDataId)` (blocking `.Result` on async)
3. **Response mapping**: `Location` → `GetExonymsResponse`
4. **HMAC signing**: `response.SignHMAC(securitySettings.HmacSigningKey)`
5. **Return**: `ActionResult` with signed response

### Error Handling

- Delegated to `NuciApiController.ProcessRequest` and middleware:
  - `NuciApiMiddleware.ExceptionHandling` → standardised error response
  - `NuciApiMiddleware.RequestLogging` → request/response logging
- Exceptions from `Gather()` propagate → 500 with error details

### Authorisation

`NuciApiAuthorisation.None` — public endpoint, no authentication required.

### Response

**Success** (200 OK):
```json
{
  "success": true,
  "defaultName": "Al Hoceima",
  "names": {
    "de": { "originalValue": "Alhucemas", "value": "Alhucemas" },
    "fr": { "originalValue": "Al Hoceïma", "value": "Al Hoceima" }
  },
  "count": 2,
  "hmac": "sha256=..."
}
```

**Error** (500):
```json
{
  "success": false,
  "error": { "code": "InternalServerError", "message": "..." }
}
```

## Request Model

### GetExonymsRequest

**File**: `ExonymsAPI/API/Requests/GetExonymsRequest.cs`
**Namespace**: `ExonymsAPI.API.Requests`
**Base**: `NuciApiRequest`

```csharp
public class GetExonymsRequest : NuciApiRequest
{
    public string GeoNamesId { get; set; }
    public string WikiDataId { get; set; }
}
```

- Simple DTO for query parameter binding
- No validation attributes (empty strings allowed)

## Response Model

### GetExonymsResponse

**File**: `ExonymsAPI/API/Responses/GetExonymsResponse.cs`
**Namespace**: `ExonymsAPI.API.Responses`
**Base**: `NuciApiSuccessResponse` (from `NuciAPI.Responses`)

```csharp
public class GetExonymsResponse : NuciApiSuccessResponse
{
    public string DefaultName { get; set; }
    public IDictionary<string, Name> Names { get; set; } = new Dictionary<string, Name>();
    public int Count => Names.Count;
}
```

**Inherited from NuciApiSuccessResponse**:
- `bool Success = true`
- `string Hmac` (set by `SignHMAC()`)

**Serialisation**:
- `Names` dictionary serialised as JSON object (language code → Name)
- `Name.Value` uses custom getter (falls back to `OriginalValue`)
- `Name.Comment` omitted when null
- `Count` computed property (not settable)

## Middleware Pipeline (from Startup.cs)

```csharp
app.UseNuciApiRequestLogging();      // Logs request/response
app.UseNuciApiExceptionHandling();   // Catches exceptions, returns standard error
app.UseDeveloperExceptionPage();     // Development only
app.UseHttpsRedirection();
app.UseDefaultFiles();
app.UseStaticFiles();
app.UseRouting();
app.UseAuthorization();              // No-op (no auth configured)
app.UseEndpoints(endpoints => endpoints.MapControllers());
```

### NuciApiRequestLogging

- Logs incoming request (method, path, headers, body)
- Logs outgoing response (status, headers, body)
- Correlation ID propagation

### NuciApiExceptionHandling

- Catches unhandled exceptions
- Returns `NuciApiErrorResponse` with `Success = false`
- Logs exception with context

## Controller Base Class

### NuciApiController

**From**: `NuciAPI.Controllers` package

Provides:
- `ProcessRequest<TRequest, TResponse>(request, handler, authorisation)`
- Authorisation handling (None, Bearer, ApiKey, etc.)
- Model validation integration
- Standardised response wrapping

## Routing

- Attribute routing: `[Route("[controller]")]` → `/Exonyms`
- No API versioning in route
- Single endpoint per controller

## Content Negotiation

- Default: JSON (ASP.NET Core default)
- No XML, no custom formatters
- `Accept` header ignored