# Browse and Search Behaviour

## User-Facing Exonym Retrieval

The Exonyms API provides a single endpoint for retrieving exonyms (names of places in different languages) for a given location identified by GeoNames ID and/or WikiData ID.

## Endpoint

```
GET /Exonyms
```

### Query Parameters

| Parameter | Type | Required | Description |
|-----------|------|----------|-------------|
| `geoNamesId` | `int` | No* | GeoNames numeric identifier |
| `wikiDataId` | `string` | No* | WikiData entity ID (e.g., `Q310350`) |

*At least one parameter must be provided.

### Example Requests

```bash
# Both IDs (recommended for best coverage)
curl "https://api.example.com/Exonyms?geoNamesId=310350&wikiDataId=Q310350"

# GeoNames only
curl "https://api.example.com/Exonyms?geoNamesId=310350"

# WikiData only
curl "https://api.example.com/Exonyms?wikiDataId=Q310350"
```

## Response Format

### Success Response (200 OK)

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
  "hmac": "sha256=abc123..."
}
```

### Response Fields

| Field | Type | Description |
|-------|------|-------------|
| `success` | `boolean` | Always `true` for 200 responses |
| `defaultName` | `string` | Primary name (English preferred) |
| `names` | `object` | Dictionary of language → name details |
| `names[lang].originalValue` | `string` | Name as returned by source (pre-transliteration) |
| `names[lang].value` | `string` | Processed name (transliterated + normalised) |
| `names[lang].comment` | `string\|null` | Provenance: "Constructed...", "Based on language...", or null |
| `count` | `integer` | Number of entries in `names` |
| `hmac` | `string` | HMAC-SHA256 signature for response verification |

### Error Responses

#### 400 Bad Request - Missing Parameters

```json
{
  "success": false,
  "error": {
    "code": "BadRequest",
    "message": "At least one of geoNamesId or wikiDataId must be provided."
  },
  "requestId": "guid"
}
```

#### 500 Internal Server Error - Upstream Failure

```json
{
  "success": false,
  "error": {
    "code": "InternalServerError",
    "message": "Failed to retrieve the WikiData entry for 'Q123': NotFound"
  },
  "requestId": "guid"
}
```

## Behavioural Guarantees

### Determinism

- **Same inputs → Same outputs** (given same external API responses)
- No randomness in processing pipeline
- GeoNames username rotation is the only non-deterministic element (affects only which account is used, not results)

### Idempotency

- **GET is idempotent** — multiple identical requests produce identical results
- No side effects (no writes, no cache invalidation)
- Safe to retry

### Completeness

| Source Provided | Result |
|-----------------|--------|
| Both IDs | Maximum coverage (merge with WikiData precedence) |
| WikiData only | Labels + sitelinks from WikiData |
| GeoNames only | Alternate names from GeoNames |

### Language Coverage

**Supported source languages**: 100+ (depends on WikiData/GeoNames data)

**Transliteration support**: 68 languages (see `TransliterationApiClient.LanguageCodes`)

**Construction**: German Middle High German (`gmh`) from German (`de`)

**Fallbacks**: 13 language groups falling back to Russian (`ru`)

### Name Processing Guarantees

1. **Transliteration**: Non-Latin scripts → Latin script (best effort)
2. **Normalisation**: Administrative suffixes removed (City, County, River, etc. in 50+ languages)
3. **Deduplication**: Names matching `defaultName` (case-insensitive) removed
4. **Sorting**: Output sorted by language code (alphabetical)

## Client Usage Patterns

### Basic Retrieval

```csharp
var request = new GetExonymsRequest
{
    GeoNamesId = 310350,
    WikiDataId = "Q310350"
};

var response = await client.GetExonymsAsync(request);

if (response.Success)
{
    Console.WriteLine($"Default: {response.DefaultName}");
    foreach (var (lang, name) in response.Names)
    {
        Console.WriteLine($"{lang}: {name.Value} (from {name.OriginalValue})");
        if (name.Comment != null)
            Console.WriteLine($"  → {name.Comment}");
    }
}
```

### HMAC Verification

```csharp
public bool VerifyResponse(GetExonymsResponse response, string hmacKey)
{
    string receivedHmac = response.Hmac;
    response.Hmac = null; // Remove before verification

    string json = JsonSerializer.Serialize(response, options);
    string computedHmac = ComputeHmacSha256(json, hmacKey);

    return receivedHmac == $"sha256={computedHmac}";
}
```

### Language Filtering

```csharp
// Get names for specific languages
var targetLanguages = new[] { "de", "fr", "es", "ru" };
var filtered = response.Names
    .Where(kvp => targetLanguages.Contains(kvp.Key))
    .ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
```

### Fallback Detection

```csharp
// Identify synthesized names
var fallbacks = response.Names
    .Where(kvp => kvp.Value.Comment?.StartsWith("Based on language") == true)
    .ToDictionary(kvp => kvp.Key, kvp => kvp.Value);

var constructed = response.Names
    .Where(kvp => kvp.Value.Comment?.StartsWith("Constructed") == true)
    .ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
```

## Rate Limiting & Quotas

**Not implemented at API level**. External dependencies have their own limits:

| Service | Limit | Notes |
|---------|-------|-------|
| GeoNames | ~2000 req/hour per username | 60+ usernames rotated |
| WikiData | No strict limit | Be respectful |
| Transliteration | Unknown | External service |

**Recommendation**: Implement client-side caching for repeated queries.

## Caching Behaviour

**Server-side**: None. Every request hits external APIs.

**Client-side recommended**:
- Cache by `(geoNamesId, wikiDataId)` key
- TTL: 24 hours (place names rarely change)
- Invalidate on 5xx errors (retry later)

## Pagination

**Not applicable**. Response contains all names for a single location. Typical count: 10-50 languages.

## Versioning

**No API versioning** currently. Single endpoint at `/Exonyms`.

Future versioning strategy (if needed):
- URL versioning: `/v2/Exonyms`
- Header versioning: `Accept: application/vnd.exonyms.v2+json`

## Discovery

**No OpenAPI/Swagger** endpoint exposed.

**Manual documentation**: This file + `api-reference/exonyms-controller.md`

## Client SDK

**None provided**. Clients use raw HTTP or generate from examples.

## Error Handling Guidance

| Error | Client Action |
|-------|---------------|
| 400 | Fix request (provide at least one ID) |
| 404 | Not used (returns 200 with empty names or 500) |
| 500 | Retry with exponential backoff (max 3) |
| 503 | Retry (upstream unavailable) |
| HMAC mismatch | Log, alert, do not trust response |

## Example: Complete Client Implementation

```csharp
public class ExonymsApiClient
{
    private readonly HttpClient _http;
    private readonly string _baseUrl;
    private readonly string _hmacKey;

    public ExonymsApiClient(HttpClient http, string baseUrl, string hmacKey)
    {
        _http = http;
        _baseUrl = baseUrl.TrimEnd('/');
        _hmacKey = hmacKey;
    }

    public async Task<ExonymsResult> GetExonymsAsync(int? geoNamesId, string wikiDataId)
    {
        var query = new List<string>();
        if (geoNamesId.HasValue) query.Add($"geoNamesId={geoNamesId.Value}");
        if (!string.IsNullOrEmpty(wikiDataId)) query.Add($"wikiDataId={wikiDataId}");

        if (query.Count == 0)
            throw new ArgumentException("At least one ID required");

        var url = $"{_baseUrl}/Exonyms?{string.Join("&", query)}";
        var response = await _http.GetAsync(url);

        if (!response.IsSuccessStatusCode)
        {
            var error = await response.Content.ReadFromJsonAsync<ErrorResponse>();
            throw new ExonymsApiException(error?.Error?.Message ?? "Unknown error");
        }

        var result = await response.Content.ReadFromJsonAsync<ExonymsResponse>();

        if (!VerifyHmac(result))
            throw new SecurityException("HMAC verification failed");

        return new ExonymsResult
        {
            DefaultName = result.DefaultName,
            Names = result.Names,
            Count = result.Count
        };
    }

    private bool VerifyHmac(ExonymsResponse response)
    {
        var hmac = response.Hmac;
        response.Hmac = null;
        var json = JsonSerializer.Serialize(response);
        var computed = ComputeHmac(json, _hmacKey);
        return hmac == $"sha256={computed}";
    }
}
```