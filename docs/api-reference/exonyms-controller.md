# Exonyms Controller

## Endpoint

```
GET /Exonyms
```

Retrieves exonyms (alternative names in different languages) for a geographic location identified by GeoNames ID and/or WikiData ID.

## Controller Class

**File**: `ExonymsAPI/API/Controllers/ExonymsController.cs`  
**Namespace**: `ExonymsAPI.API.Controllers`  
**Base Class**: `NuciApiController` (from `NuciAPI.Controllers`)

```csharp
[Route("[controller]")]
public class ExonymsController : NuciApiController
{
    private readonly IExonymsService exonymsService;
    private readonly SecuritySettings securitySettings;

    public ExonymsController(IExonymsService exonymsService, SecuritySettings securitySettings)
    {
        this.exonymsService = exonymsService;
        this.securitySettings = securitySettings;
    }

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
}
```

## Request Model

### GetExonymsRequest

**File**: `ExonymsAPI/API/Requests/GetExonymsRequest.cs`  
**Namespace**: `ExonymsAPI.API.Requests`

```csharp
public class GetExonymsRequest
{
    public int? GeoNamesId { get; set; }
    public string WikiDataId { get; set; }
}
```

### Query Parameters

| Parameter | Type | Required | Description |
|-----------|------|----------|-------------|
| `geoNamesId` | `integer` | No* | GeoNames numeric identifier (e.g., `310350`) |
| `wikiDataId` | `string` | No* | WikiData entity ID (e.g., `Q310350`) |

*At least one parameter must be provided.

### Validation Rules

- If both `geoNamesId` and `wikiDataId` are null/empty → **400 Bad Request**
- `geoNamesId` must be a positive integer if provided
- `wikiDataId` must match pattern `^Q\d+$` if provided (not enforced by model, validated by upstream)

### Example Requests

```bash
# Both IDs (recommended)
GET /Exonyms?geoNamesId=310350&wikiDataId=Q310350

# GeoNames only
GET /Exonyms?geoNamesId=310350

# WikiData only
GET /Exonyms?wikiDataId=Q310350

# Invalid - missing both
GET /Exonyms
```

## Response Model

### GetExonymsResponse

**File**: `ExonymsAPI/API/Responses/GetExonymsResponse.cs`  
**Namespace**: `ExonymsAPI.API.Responses`  
**Base Class**: `NuciApiSuccessResponse` (from `NuciAPI.Controllers`)

```csharp
public class GetExonymsResponse : NuciApiSuccessResponse
{
    public string DefaultName { get; set; }
    public Dictionary<string, Name> Names { get; set; }
    public int Count => Names?.Count ?? 0;
}
```

### Name Model

**File**: `ExonymsAPI/Service/Models/Name.cs`  
**Namespace**: `ExonymsAPI.Service.Models`

```csharp
public class Name
{
    public string OriginalValue { get; set; }
    public string Value { get; set; }
    public string Comment { get; set; }

    public Name(string originalValue)
    {
        OriginalValue = originalValue;
        Value = originalValue;
    }

    public bool Equals(string other)
        => string.Equals(Value, other, StringComparison.OrdinalIgnoreCase);
}
```

### Response Fields

| Field | Type | Description |
|-------|------|-------------|
| `success` | `boolean` | Always `true` for successful responses |
| `defaultName` | `string` | Primary name (English preferred, from WikiData or GeoNames) |
| `names` | `object` | Dictionary mapping language codes to name details |
| `names[lang].originalValue` | `string` | Original name from source (pre-transliteration) |
| `names[lang].value` | `string` | Processed name (transliterated to Latin script + normalised) |
| `names[lang].comment` | `string\|null` | Provenance info: "Constructed...", "Based on language...", or null |
| `count` | `integer` | Number of entries in `names` (computed property) |
| `hmac` | `string` | HMAC-SHA256 signature for response verification |

### Success Response Example (200 OK)

```json
{
  "success": true,
  "defaultName": "Al Hoceima",
  "names": {
    "ar": {
      "originalValue": "الحسيمة",
      "value": "Al Hoceima",
      "comment": null
    },
    "de": {
      "originalValue": "Alhucemas",
      "value": "Alhucemas",
      "comment": null
    },
    "gmh": {
      "originalValue": "Alhucemas",
      "value": "Alhukemas",
      "comment": "Constructed. Based on language 'de'"
    },
    "ru": {
      "originalValue": "Аль-Хосейма",
      "value": "Al-Khoseyma",
      "comment": null
    },
    "tt": {
      "originalValue": "Аль-Хосейма",
      "value": "Al-Khoseyma",
      "comment": "Based on language 'ru'"
    }
  },
  "count": 5,
  "hmac": "sha256=abc123def456..."
}
```

## Error Responses

### 400 Bad Request - Missing Parameters

```json
{
  "success": false,
  "error": {
    "code": "BadRequest",
    "message": "At least one of geoNamesId or wikiDataId must be provided."
  },
  "requestId": "a1b2c3d4-e5f6-7890-abcd-ef1234567890"
}
```

### 500 Internal Server Error - Upstream Failure

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

### 500 Internal Server Error - Processing Error

```json
{
  "success": false,
  "error": {
    "code": "InternalServerError",
    "message": "An error occurred while processing the request."
  },
  "requestId": "a1b2c3d4-e5f6-7890-abcd-ef1234567890"
}
```

## HMAC Signing

All successful responses are signed with HMAC-SHA256 for integrity verification.

### Signing Process

```csharp
// In controller
response.SignHMAC(securitySettings.HmacSigningKey);
```

### Implementation (NuciSecurity.HMAC)

```csharp
public static void SignHMAC(this NuciApiSuccessResponse response, string key)
{
    string json = JsonSerializer.Serialize(response, options); // Excludes Hmac property
    string hmac = ComputeHmacSha256(json, key);
    response.Hmac = $"sha256={hmac}";
}
```

### Verification (Client Side)

```csharp
public bool VerifyHmac(GetExonymsResponse response, string key)
{
    string receivedHmac = response.Hmac;
    response.Hmac = null;

    string json = JsonSerializer.Serialize(response, options);
    string computedHmac = ComputeHmacSha256(json, key);

    return receivedHmac == $"sha256={computedHmac}";
}
```

### HMAC Details

| Property | Value |
|----------|-------|
| Algorithm | HMAC-SHA256 |
| Key | `SecuritySettings.HmacSigningKey` (from configuration) |
| Format | `sha256=<base64>` |
| Signed Content | Full JSON response excluding `hmac` field |

## Processing Pipeline

The controller delegates to `ExonymsService.Gather()` which performs:

1. **WikiData Gathering** - Fetch labels and sitelinks from WikiData
2. **GeoNames Gathering** - Fetch alternate names from GeoNames
3. **Merge** - Combine with WikiData precedence
4. **Construct** - Generate German Middle High German (`gmh`) from German (`de`)
5. **Fallbacks** - Synthesize 13 language groups from Russian (`ru`)
6. **Deduplicate** - Remove names matching `defaultName`
7. **Sort** - Alphabetical by language code

See [Exonym Gathering Flow](../flows/exonym-gathering.md) for details.

## Authorization

**None required**. The endpoint uses `NuciApiAuthorisation.None`.

```csharp
ProcessRequest(request, handler, NuciApiAuthorisation.None);
```

## Request Logging

All requests/responses are logged via `NuciApiRequestLogging` middleware:

- Request: method, path, query, headers, correlation ID
- Response: status code, duration, response body (truncated)

## Exception Handling

Unhandled exceptions are caught by `NuciApiExceptionHandling` middleware:

- Logged with correlation ID
- Returns standardized error response
- Status code: 500 (or 400 for validation errors)

## Performance

| Metric | Typical Value |
|--------|---------------|
| Latency (both IDs) | 300-800ms |
| Latency (single ID) | 150-400ms |
| Response size | 2-10 KB |
| External calls | 2-3 (WikiData, GeoNames, Transliteration × N) |

## Caching

**No server-side caching**. Every request executes the full pipeline.

### Recommended Client Caching

```csharp
// Cache key: (geoNamesId, wikiDataId)
// TTL: 24 hours
// Invalidate on 5xx errors
```

## Rate Limiting

**No server-side rate limiting**. External services have their own limits:

| Service | Limit |
|---------|-------|
| GeoNames | ~2000 req/hour per username (60+ usernames rotated) |
| WikiData | No strict limit |
| Transliteration | Service-dependent |

## Versioning

**No versioning** currently. Single endpoint at `/Exonyms`.

## OpenAPI/Swagger

**Not exposed**. No Swagger/OpenAPI endpoint configured.

## Testing

### Unit Tests

**File**: `ExonymsAPI.UnitTests/Service/ExonymsServiceTests.cs`

Tests cover:
- Merge logic (WikiData precedence)
- Construction (gmh from de)
- Fallbacks (13 language groups)
- Deduplication
- Sorting

### Integration Tests

**None currently**. Would require:
- Mock HTTP handlers for WikiData/GeoNames
- Testcontainers for full stack

## Related Documentation

- [API Reference Index](INDEX.md)
- [Exonym Gathering Flow](../flows/exonym-gathering.md)
- [Browse and Search Behaviour](../behaviour/browse-and-search.md)
- [Components: Application Services](../components/application-services.md)
- [Components: Integration Models](../components/integration-models.md)
- [Configuration](../configuration.md)
- [Security](../security.md)