# Integration Models

## External API Request/Response Models

### Transliteration API

#### GetTransliterationsRequest

**File**: `ExonymsAPI/Client/TransliterationAPI/Requests/GetTransliterationsRequest.cs`  
**Namespace**: `ExonymsAPI.Client.TransliterationAPI.Requests`

```csharp
public class GetTransliterationsRequest
{
    public string Text { get; set; }
    public string Language { get; set; }
}
```

**Usage**: Sent as query parameters to transliteration endpoint.

| Field | Type | Description |
|-------|------|-------------|
| `Text` | `string` | Name to transliterate |
| `Language` | `string` | Source language code (e.g., `ru`, `ar`, `ja`) |

#### GetTransliterationsResponse

**File**: `ExonymsAPI/Client/TransliterationAPI/Responses/GetTransliterationsResponse.cs`  
**Namespace**: `ExonymsAPI.Client.TransliterationAPI.Responses`

```csharp
public class GetTransliterationsResponse
{
    public string Text { get; set; }
}
```

**Usage**: Parsed from JSON response body.

| Field | Type | Description |
|-------|------|-------------|
| `Text` | `string` | Transliterated name in Latin script |

### NuciAPI Envelope

Both request/response are wrapped by `NuciApiClient.SendRequestAsync` in a NuciAPI envelope:

**Request envelope** (sent):
```json
{
  "request": { "Text": "Москва", "Language": "ru" },
  "requestId": "guid",
  "timestamp": "2026-10-08T12:00:00Z"
}
```

**Response envelope** (received):
```json
{
  "success": true,
  "response": { "Text": "Moskva" },
  "requestId": "guid",
  "timestamp": "2026-10-08T12:00:00Z"
}
```

## GeoNames Integration

### Request

**Endpoint**: `http://api.geonames.org/get`

**Parameters**:
| Parameter | Value | Source |
|-----------|-------|--------|
| `geonameId` | `{geoNamesId}` | Method parameter |
| `username` | Random from hardcoded list | `GeoNamesGatherer.usernames` |

**Example**: `http://api.geonames.org/get?geonameId=310350&username=geonamesfreeaccountt`

### Response (XML)

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

### Parsing Logic

```csharp
XDocument doc = XDocument.Parse(xml);
XElement geonameElement = doc.Root;

location.DefaultName = (string)geonameElement.Element("name");
location.DefaultName = nameNormaliser.Normalise("en", location.DefaultName);

IEnumerable<XElement> alternateNameElements = geonameElement.Elements("alternateName");

foreach (XElement alternateNameElement in alternateNameElements)
{
    string languageCode = alternateNameElement.Attribute("lang")?.Value;

    if (string.IsNullOrWhiteSpace(languageCode) ||
        location.Names.ContainsKey(languageCode) ||
        IgnoredLanguageCodes.Contains(languageCode))
    {
        continue;
    }

    Name name = new(alternateNameElement.Value);
    name.Value = await transliterationApiClient.Transliterate(languageCode, name.Value);
    name.Value = nameNormaliser.Normalise(languageCode, name.Value);
    location.Names.Add(languageCode, name);
}
```

### Ignored Language Codes

```csharp
private static string[] IgnoredLanguageCodes => ["link", "unlc", "wkdt"];
```

- `link`: External URLs
- `unlc`: Unclassified/unknown
- `wkdt`: WikiData references

## WikiData Integration

### Request

**Endpoint**: `https://wikidata.org/wiki/Special:EntityData/{wikiDataId}.json`

**Parameter**: `wikiDataId` in path (e.g., `Q310350`)

**Example**: `https://wikidata.org/wiki/Special:EntityData/Q310350.json`

### Response (JSON)

```json
{
  "entities": {
    "Q310350": {
      "labels": {
        "en": { "value": "Al Hoceima" },
        "ar": { "value": "الحسيمة" },
        "de": { "value": "Alhucemas" },
        "fr": { "value": "Al Hoceïma" }
      },
      "sitelinks": {
        "dewiki": { "title": "Alhucemas" },
        "frwiki": { "title": "Al Hoceïma" },
        "enwiki": { "title": "Al Hoceima" },
        "ruwiki": { "title": "Аль-Хосейма" }
      }
    }
  }
}
```

### Parsing Logic

```csharp
JObject data = JObject.Parse(json);
JObject entities = (JObject)data["entities"];
JObject entity = (JObject)entities[wikiDataId];
JObject labels = (JObject)entity["labels"];
JObject sitelinks = (JObject)entity["sitelinks"];

// Labels
if (labels.TryGetValue(DefaultNameLanguageCode, out var defaultLabel))
{
    location.DefaultName = (string)defaultLabel["value"];
    location.DefaultName = nameNormaliser.Normalise(DefaultNameLanguageCode, location.DefaultName);
}

foreach (var label in labels)
{
    string languageCode = label.Key;
    Name name = new((string)label.Value["value"]);
    name.Value = await transliterationApiClient.Transliterate(languageCode, name.OriginalValue);
    name.Value = nameNormaliser.Normalise(languageCode, name.Value);
    location.Names.Add(languageCode, name);
}

// Sitelinks
foreach (var sitelink in sitelinks)
{
    string languageCode = Regex.Replace(sitelink.Key, @"(news|quote|source|voyage|wiki)", "");

    if (location.Names.ContainsKey(languageCode))
        continue;

    Name name = new((string)sitelink.Value["title"]);
    name.Value = await transliterationApiClient.Transliterate(languageCode, name.OriginalValue);
    name.Value = nameNormaliser.Normalise(languageCode, name.Value);

    if (name.Equals(location.DefaultName))
        continue;

    location.Names.Add(languageCode, name);
}
```

### Sitelink Language Extraction

**Regex**: `@"(news|quote|source|voyage|wiki)"` → replaced with empty string

| Sitelink Key | Extracted Language |
|--------------|-------------------|
| `dewiki` | `de` |
| `enwikinews` | `en` |
| `frwikiquote` | `fr` |
| `ruwikisource` | `ru` |
| `devoyage` | `de` |

**Note**: Heuristic; may produce incorrect codes for non-standard sitelinks.

## Data Transformation Pipeline

### Per-Name Processing (Both Gatherers)

```
Source Value (OriginalValue)
    │
    ▼
TransliterationApiClient.Transliterate(languageCode, OriginalValue)
    │
    ├── Language in transliteration list → API call → Transliterated text
    └── Language NOT in list → Return OriginalValue unchanged
    │
    ▼
NameNormaliser.Normalise(languageCode, TransliteratedValue)
    │
    ├── Generic patterns (suffixes, prefixes, quotes, XML tags)
    ├── Generic "of" words (20+ languages)
    ├── Language-specific patterns (50+ languages × 15+ categories)
    └── Cleanup (whitespace, dashes, trim)
    │
    ▼
Processed Value (Name.Value)
```

### Fallback Synthesis Processing

```
Missing Language (e.g., "ab")
    │
    ▼
Find first available fallback language (e.g., "ru" from languageFallbacks["ab"])
    │
    ▼
Take fallback's OriginalValue (e.g., "Москва")
    │
    ▼
Transliterate to missing language (Transliterate("ab", "Москва"))
    │
    ├── Result ≠ OriginalValue → Use result
    └── Result = OriginalValue (script unchanged) → Transliterate from fallback lang (Transliterate("ru", "Москва"))
    │
    ▼
Normalise with fallback language code (Normalise("ru", Result))
    │
    ▼
Add to Names with Comment: "Based on language 'ru'"
```

### Construction Processing

```
Target Language (e.g., "gmh") not in Names
    │
    ▼
Find first available base language (e.g., "de" from languagesToConstruct["gmh"])
    │
    ▼
Take base language's processed Value (e.g., "Alhucemas")
    │
    ▼
NameConstructor.Construct(Value, "gmh")
    │
    ├── Apply gmh transformation rules (70+ regex patterns)
    └── Return constructed form
    │
    ▼
Add to Names with Comment: "Constructed. Based on language 'de'"
```

## Error Models

### GeoNames Errors

| Condition | Exception | Logged |
|-----------|-----------|--------|
| HTTP non-2xx | `HttpRequestException` | Yes (Error, Failure) |
| XML parse error | `XmlException` | Yes |
| Missing `<name>` | Null reference | No (would throw) |

### WikiData Errors

| Condition | Exception | Logged |
|-----------|-----------|--------|
| HTTP non-2xx | `HttpRequestException` | Yes (Error, Failure) |
| JSON parse error | `JsonReaderException` | Yes |
| Missing entity | Returns empty Location | No (silent) |
| Missing labels | Empty Names | No (silent) |

### Transliteration Errors

| Condition | Behaviour | Logged |
|-----------|-----------|--------|
| Language not supported | Return original | No |
| API HTTP error | Return original | No (in NuciApiClient) |
| API unsuccessful envelope | Return original | No |
| Empty response.Text | Return original | No |

### Propagation

All gatherer exceptions propagate to `ExonymsService.Gather()` → `ExonymsController` → `NuciApiExceptionHandling` middleware → 500 response.

## Integration Configuration

### TransliterationSettings

```csharp
public class TransliterationSettings
{
    public string TransliterationApiBaseUrl { get; set; }
}
```

**Used by**: `NuciApiClient` constructor (via factory in `ServiceCollectionExtensions`)

**Example**: `https://api.nucilandia.ro/translit`

### GeoNames Usernames

**Location**: `GeoNamesGatherer.usernames` (private readonly field)

**Count**: 60+ accounts

**Selection**: `usernames.GetRandomElement()` (NuciExtensions)

**Security Note**: Hardcoded in source — consider moving to configuration/secrets.

## Integration Test Considerations

### Mocking Strategy (from UnitTests)

- `IGeoNamesGatherer`, `IWikiDataGatherer` mocked at service level
- `ITransliterationApiClient` mocked for transliteration behaviour
- `INameNormaliser`, `INameConstructor` mocked for transformation behaviour
- `HttpClient` not mocked directly (gatherers create own instances)

### Integration Test Gaps

No integration tests for:
- Actual GeoNames HTTP calls
- Actual WikiData HTTP calls
- Actual Transliteration API calls
- End-to-end pipeline with real services

### Contract Testing Opportunities

- GeoNames XML schema validation
- WikiData JSON schema validation
- Transliteration API envelope format
- Language code mapping consistency