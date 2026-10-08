# Data Model

## Domain Entities

### Location

**Namespace**: `ExonymsAPI.Service.Models`
**File**: `Service/Models/Location.cs`

```csharp
public class Location(string defaultName)
{
    public string DefaultName { get; set; } = defaultName;
    public IDictionary<string, Name> Names { get; set; } = new Dictionary<string, Name>();

    public Location() : this(null) { }
}
```

**Purpose**: Aggregates all names for a geographical location.

**Properties**:
| Property | Type | Description |
|----------|------|-------------|
| `DefaultName` | `string` | Primary name (English preferred, from WikiData labels or GeoNames name) |
| `Names` | `IDictionary<string, Name>` | Language code → Name mapping (exonyms) |

**Invariants**:
- `Names` keys are unique language codes (ISO 639-1/2/3, with variants like `sr-ec`)
- `DefaultName` may be null if no source provides it
- `Names` does not contain the default language (`en`) as a separate entry

**Lifecycle**:
1. Created empty by gatherers
2. Populated with default name and alternate names
3. Merged in `ExonymsService.Gather()` (WikiData first, then GeoNames)
4. Processed: `ConstructNames()` → `ApplyFallbacks()` → `RemoveRedundantExonyms()`
5. Sorted alphabetically by language code
6. Returned to controller for response mapping

### Name

**Namespace**: `ExonymsAPI.Service.Models`
**File**: `Service/Models/Name.cs`

```csharp
public class Name(string name)
{
    public string OriginalValue { get; set; } = name;
    public string Value { get; set; } // Returns OriginalValue if null/empty
    public string Comment { get; set; } // JsonIgnore when null

    public static bool IsNullOrWhiteSpace(Name name)
}
```

**Purpose**: Represents a single exonym with metadata.

**Properties**:
| Property | Type | Description |
|----------|------|-------------|
| `OriginalValue` | `string` | Raw name from source (before transliteration/normalisation) |
| `Value` | `string` | Processed name (transliterated + normalised); falls back to `OriginalValue` |
| `Comment` | `string?` | Optional metadata (e.g., "Based on language 'ru'", "Constructed. Based on language 'de'") |

**Behaviour**:
- `Value` getter returns `OriginalValue` if `Value` is null/whitespace
- `Comment` is omitted from JSON when null (`JsonIgnoreCondition.WhenWritingNull`)
- `IsNullOrWhiteSpace()` static helper checks null, empty, or whitespace `Value`

**Usage in Flow**:
1. Created by gatherers with `OriginalValue` from source
2. `Value` set via transliteration → normalisation pipeline
3. `Comment` added during fallback synthesis or construction
4. Serialised in `GetExonymsResponse.Names`

## Relationships

```
Location (1) ──────► (0..*) Name
    │
    └── DefaultName (string, not in Names dictionary)
```

- `Location.DefaultName` is the canonical English name
- `Location.Names` contains exonyms for other languages
- Language codes are dictionary keys (e.g., "de", "fr", "sr-ec", "gmh")

## Data Flow Transformations

### Source → Location (Gatherers)

**GeoNamesGatherer**:
```
XML <geoname>
  → DefaultName = <name> (normalised for "en")
  → Names[lang] = <alternateName lang="lang"> (transliterated → normalised)
```

**WikiDataGatherer**:
```
JSON entities.{id}.labels
  → DefaultName = labels["en"].value (normalised for "en")
  → Names[lang] = labels[lang].value (transliterated → normalised)

JSON entities.{id}.sitelinks
  → Names[lang] = sitelinks[lang+"wiki"].title (transliterated → normalised)
  → lang extracted by removing suffix (wiki|news|quote|source|voyage)
```

### Location → Location (ExonymsService Processing)

1. **Merge** (WikiData first, then GeoNames):
   - `DefaultName`: First non-empty wins (WikiData precedence)
   - `Names`: First occurrence wins (WikiData precedence)

2. **ConstructNames** (`ConstructNames()`):
   - For each `languageToConstruct` (currently `gmh` from `de`):
     - If `languageToConstruct` not in `Names` but `baseLanguage` is:
       - `NameConstructor.Construct(baseName.Value, languageToConstruct)`
       - Add with `Comment = "Constructed. Based on language '{baseLanguage}'"`

3. **ApplyFallbacks** (`ApplyFallbacks()`):
   - For each `languageToFallbackFrom` missing in `Names`:
     - Find first `languageToFallbackTo` present in `Names` from fallback chain
     - Transliterate `OriginalValue` to `languageToFallbackFrom`
     - If result equals `OriginalValue`, try transliterating from `languageToFallbackTo`
     - Normalise result
     - Add with `Comment = "Based on language '{languageToFallbackTo}'"`

4. **RemoveRedundantExonyms** (`RemoveRedundantExonyms()`):
   - Remove any `Names[lang]` where `lang != "en"` and `Value == DefaultName`

5. **Sort**: `Names = Names.OrderBy(x => x.Key).ToDictionary(...)`

## Response Model

### GetExonymsResponse

**Namespace**: `ExonymsAPI.API.Responses`
**File**: `API/Responses/GetExonymsResponse.cs`

```csharp
public class GetExonymsResponse : NuciApiSuccessResponse
{
    public string DefaultName { get; set; }
    public IDictionary<string, Name> Names { get; set; } = new Dictionary<string, Name>();
    public int Count => Names.Count;
}
```

**Mapping from Location** (in `ExonymsController.Get()`):
```csharp
GetExonymsResponse response = new()
{
    DefaultName = exonyms.DefaultName,
    Names = exonyms.Names
};
response.SignHMAC(securitySettings.HmacSigningKey);
```

**JSON Output Example**:
```json
{
  "success": true,
  "defaultName": "Al Hoceima",
  "names": {
    "ar": { "originalValue": "الحسيمة", "value": "Al Hoceima", "comment": null },
    "de": { "originalValue": "Alhucemas", "value": "Alhucemas", "comment": null },
    "fr": { "originalValue": "Al Hoceïma", "value": "Al Hoceima", "comment": null },
    "gmh": { "originalValue": "Alhucemas", "value": "Alhucemas", "comment": "Constructed. Based on language 'de'" },
    "ru": { "originalValue": "Аль-Хосейма", "value": "Al'Khozejma", "comment": null }
  },
  "count": 5,
  "hmac": "sha256=..."
}
```

## Language Codes

### Supported Sources

| Source | Language Code Format | Examples |
|--------|---------------------|----------|
| GeoNames | ISO 639-1/2/3 + variants | `en`, `de`, `fr`, `sr-ec`, `zh` |
| WikiData labels | ISO 639-1/2/3 | `en`, `de`, `fr`, `ru`, `zh` |
| WikiData sitelinks | Extracted from key suffix | `dewiki` → `de`, `enwikinews` → `en` |

### Special Codes

| Code | Language | Source |
|------|----------|--------|
| `gmh` | German Middle High German | Constructed from `de` |
| `sr-ec` | Serbian Cyrillic | WikiData/GeoNames |
| `tg-cyrl` | Tajik Cyrillic | Transliteration target |
| `tt-cyrl` | Tatar Cyrillic | Transliteration target |
| `zh-hans` | Simplified Chinese | Transliteration target |

### Fallback Chains (from `ExonymsService.languageFallbacks`)

```csharp
"ab" → ["ru", "uk", "be", "tg", "kk", "tt", "cv", "bg", "mk", "sr-ec", "cu"]
"be" → ["ru", "uk", "cu", "bg", "mk", "sr-ec", "tt", "kk", "cv", "tg", "ab"]
"bg" → ["ru", "mk", "sr-ec", "cu", "uk", "be", "tt", "cv", "kk", "tg", "ab"]
// ... 13 language groups total
```

### Construction Chains (from `ExonymsService.languagesToConstruct`)

```csharp
"gmh" → ["de"]  // Middle High German from modern German
```

## Transliteration Target Languages

From `TransliterationApiClient.languageCodesToTransliterate` (68 languages):

**Cyrillic**: `ru`, `uk`, `be`, `bg`, `mk`, `sr-ec`, `kk`, `ky`, `mn`, `tg-cyrl`, `tt-cyrl`, `cv`, `ba`, `os`, `udm`

**Arabic**: `ar`, `ary`, `arz`

**Other non-Latin**: `el`, `grc`, `grc-dor`, `he`, `hi`, `hy`, `hyw`, `ja`, `ka`, `ko`, `th`, `zh`, `zh-hans`, `bn`, `gu`, `kn`, `ml`, `mr`, `ta`, `te`, `sa`, `iu`, `cop`, `cu`, `sh`, `si`

**Latin-script but transliterated**: `ab`, `ady`, `ba`, `cv`, `sh`, `sr` (may contain non-ASCII)

## Serialisation Notes

- `Name.Value` uses custom getter: returns `OriginalValue` if `Value` not set
- `Name.Comment` omitted when null (`JsonIgnoreCondition.WhenWritingNull`)
- `GetExonymsResponse.Count` is computed property (`Names.Count`)
- `GetExonymsResponse` inherits `NuciApiSuccessResponse` (adds `Success = true`, `Hmac` property)
- HMAC added via `SignHMAC(key)` method from `NuciSecurity.HMAC`