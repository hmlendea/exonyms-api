# Application Services

## ExonymsService

**File**: `ExonymsAPI/Service/ExonymsService.cs`  
**Namespace**: `ExonymsAPI.Service`  
**Interface**: `IExonymsService`

### Purpose

Core orchestration service that coordinates gathering exonyms from multiple sources, merges results, applies transformations (construction, fallbacks, deduplication), and returns the final `Location` object.

### Dependencies (Constructor Injection)

| Dependency | Interface | Lifetime | Purpose |
|------------|-----------|----------|---------|
| `geoNamesGatherer` | `IGeoNamesGatherer` | Singleton | Fetch from GeoNames |
| `wikiDataGatherer` | `IWikiDataGatherer` | Singleton | Fetch from WikiData |
| `nameConstructor` | `INameConstructor` | Singleton | Construct historical variants |
| `transliterationApiClient` | `ITransliterationApiClient` | Transient | Transliterate fallback/constructed names |
| `nameNormaliser` | `INameNormaliser` | Singleton | Normalise all names |
| `logger` | `ILogger` | Transient | Structured logging |

### Public API

```csharp
public async Task<Location> Gather(string geoNamesId, string wikiDataId)
```

**Parameters**:
- `geoNamesId`: GeoNames feature ID (optional, empty string if not provided)
- `wikiDataId`: WikiData entity ID (optional, empty string if not provided)

**Returns**: `Location` with `DefaultName` and `Names` dictionary

**Throws**: Exceptions from gatherers propagate (HTTP errors, parsing errors)

### Algorithm

```csharp
public async Task<Location> Gather(string geoNamesId, string wikiDataId)
{
    // 1. Log start
    logger.Info(MyOperation.GatherExonyms, OperationStatus.Started, logInfos);

    // 2. Gather from sources (sequential, WikiData first)
    IList<Location> gatheredLocations = [];
    if (!string.IsNullOrWhiteSpace(wikiDataId))
        gatheredLocations.Add(await wikiDataGatherer.Gather(wikiDataId));
    if (!string.IsNullOrWhiteSpace(geoNamesId))
        gatheredLocations.Add(await geoNamesGatherer.Gather(geoNamesId));

    // 3. Merge locations
    Location location = new();
    foreach (Location gatheredLocation in gatheredLocations)
    {
        if (string.IsNullOrWhiteSpace(location.DefaultName))
            location.DefaultName = gatheredLocation.DefaultName;

        foreach (var name in gatheredLocation.Names.Where(x => !location.Names.ContainsKey(x.Key)))
            location.Names.TryAdd(name.Key, name.Value);
    }

    // 4. Post-processing pipeline
    location = ConstructNames(location);      // Historical variants
    location = await ApplyFallbacks(location); // Synthesise missing languages
    location = RemoveRedundantExonyms(location); // Remove duplicates of default

    // 5. Sort alphabetically by language code
    location.Names = location.Names
        .OrderBy(x => x.Key)
        .ToDictionary(x => x.Key, x => x.Value);

    // 6. Log success
    logger.Info(MyOperation.GatherExonyms, OperationStatus.Success, logInfos);

    return location;
}
```

### Merge Logic

**Precedence**: WikiData first → GeoNames second

| Field | Merge Rule |
|-------|------------|
| `DefaultName` | First non-empty wins (WikiData priority) |
| `Names[lang]` | First occurrence wins (WikiData priority) |

**Implementation**: `TryAdd` only adds if key doesn't exist.

### Post-Processing Pipeline

#### 1. ConstructNames

```csharp
private Location ConstructNames(Location location)
{
    foreach (string language in languagesToConstruct.Keys.Where(l => !location.Names.ContainsKey(l)))
    {
        foreach (string baseLanguage in languagesToConstruct[language].Where(location.Names.ContainsKey))
        {
            Name name = new(location.Names[baseLanguage].OriginalValue)
            {
                Comment = $"Constructed. Based on language '{baseLanguage}'",
                Value = nameConstructor.Construct(location.Names[baseLanguage].Value, language)
            };
            location.Names.Add(language, name);
            break; // Only first available base language
        }
    }
    return location;
}
```

**Configuration** (`languagesToConstruct`):
```csharp
{ "gmh", ["de"] }  // Middle High German from German
```

**Behaviour**: For each target language not present, find first available base language, construct variant, add with comment.

#### 2. ApplyFallbacks

```csharp
private async Task<Location> ApplyFallbacks(Location location)
{
    foreach (string languageToFallbackFrom in languageFallbacks.Keys.Where(l => !location.Names.ContainsKey(l)))
    {
        foreach (string languageToFallbackTo in languageFallbacks[languageToFallbackFrom].Where(location.Names.ContainsKey))
        {
            Name name = new(location.Names[languageToFallbackTo].OriginalValue)
            {
                Comment = $"Based on language '{languageToFallbackTo}'"
            };

            name.Value = await transliterationApiClient.Transliterate(languageToFallbackFrom, name.OriginalValue);

            if (name.Value.Equals(name.OriginalValue))
            {
                name.Value = await transliterationApiClient.Transliterate(languageToFallbackTo, name.OriginalValue);
            }

            name.Value = nameNormaliser.Normalise(languageToFallbackTo, name.Value);
            location.Names.Add(languageToFallbackFrom, name);
            break; // Only first available fallback language
        }
    }
    return location;
}
```

**Configuration** (`languageFallbacks`): 13 language groups, e.g.:
```csharp
"ab" → ["ru", "uk", "be", "tg", "kk", "tt", "cv", "bg", "mk", "sr-ec", "cu"]
"ru" → ["uk", "be", "cu", "bg", "mk", "sr-ec", "tt", "kk", "cv", "tg", "ab"]
```

**Behaviour**: For each missing language, find first available fallback language, transliterate from fallback's `OriginalValue`. If transliteration returns same script, try transliterating from fallback language code. Normalise result.

#### 3. RemoveRedundantExonyms

```csharp
private Location RemoveRedundantExonyms(Location location)
{
    foreach (string language in location.Names.Keys.Where(l => !string.IsNullOrWhiteSpace(location.Names[l].Value)))
    {
        if (!language.Equals(WikiDataGatherer.DefaultNameLanguageCode) &&
            location.Names[language].Value.Equals(location.DefaultName))
        {
            location.Names.Remove(language);
        }
    }
    return location;
}
```

**Behaviour**: Remove any exonym (except `en`) whose processed `Value` equals `DefaultName`.

### Logging

**Operations** (from `MyOperation`):
- `GatherExonyms` — Main orchestration
- `GatherGeoNamesExonyms` — Delegated to GeoNamesGatherer
- `GatherWikiDataExonyms` — Delegated to WikiDataGatherer

**Log Info Keys** (from `MyLogInfoKey`):
- `GeoNamesId`, `WikiDataId` — Input identifiers
- `DefaultName`, `Count` — Result summary

**Levels**:
- `Info` with `OperationStatus.Started` — On entry
- `Info` with `OperationStatus.Success` — On completion
- `Error` with `OperationStatus.Failure` — On exception (in gatherers)

### Error Handling

- Exceptions from gatherers propagate up (not caught in `Gather()`)
- Controller middleware catches and returns 500
- Logging in gatherers captures failure context

### Thread Safety

- Stateless: No instance fields modified after construction
- All dependencies are thread-safe (singletons are immutable, transient per-request)
- Safe for concurrent calls

## IExonymsService

**File**: `ExonymsAPI/Service/IExonymsService.cs`

```csharp
public interface IExonymsService
{
    Task<Location> Gather(string geoNamesId, string wikiDataId);
}
```

Single-method interface for testability and decoupling.

## NameNormaliser

**File**: `ExonymsAPI/Service/Processors/NameNormaliser.cs`  
**Interface**: `INameNormaliser`

### Purpose

Removes administrative/geographic suffixes, prefixes, and noise from names using regex patterns.

### Algorithm

```csharp
public string Normalise(string languageCode, string name)
{
    string normalisedName = name;

    // 1. Generic pattern removal (suffixes, prefixes, quotes, etc.)
    normalisedName = RemoveTextPattern(normalisedName, " - .*");
    normalisedName = RemoveTextPattern(normalisedName, ",.*");
    normalisedName = RemoveTextPattern(normalisedName, "[…]");
    normalisedName = RemoveTextPattern(normalisedName, "/.*");
    normalisedName = RemoveTextPattern(normalisedName, "\\(.*");
    normalisedName = RemoveTextPattern(normalisedName, @"<alternateName[^>]*>.*$");
    normalisedName = RemoveTextPattern(normalisedName, "^\"(.*)\"$", "$1");
    normalisedName = RemoveTextPattern(normalisedName, @"^[^\s]*:");

    // 2. Generic word removal (of, the, etc. in many languages)
    normalisedName = RemoveWords(normalisedName);

    // 3. Language-specific word removal
    normalisedName = RemoveLanguageSpecificWords(languageCode, normalisedName);

    // 4. Cleanup
    normalisedName = RemoveTextPattern(normalisedName, @"\s\s*", " ");
    normalisedName = RemoveTextPattern(normalisedName, @"^[\s\-]*");
    normalisedName = RemoveTextPattern(normalisedName, @"[\s\-]*$");
    normalisedName = normalisedName.Trim();

    return normalisedName;
}
```

### Pattern Categories

1. **Generic patterns**: Suffixes after delimiters (` - `, `,`, `/`, `(`, quotes, XML tags, prefixes)
2. **Generic words**: "of" equivalents in 20+ languages (`of`, `de`, `da`, `di`, `van`, `von`, etc.)
3. **Language-specific**: 50+ language blocks for administrative terms:
   - Peninsula, Abbey, Agency, Airport, Ancient, Area, Autonomous Government
   - Castle, City, Cliff, County, District, Division, Duchy, Emirate, Fort
   - Island, Kingdom, Lake, Marquisate, Monastery, Municipality, Peninsula
   - Prefecture, Province, River, State, Town, Township, Valley
   - Cathedral, Church, Mountain, etc.

### Language-Specific Handling

- `languageCode` parameter selects which language-specific patterns to apply
- Patterns defined as regex alternations per language
- Empty `languageCode` skips language-specific patterns

### Thread Safety

- Stateless, pure function
- No shared state
- Safe for concurrent use

## NameConstructor

**File**: `ExonymsAPI/Service/Processors/NameConstructor.cs`  
**Interface**: `INameConstructor`

### Purpose

Constructs historical language variants by applying transformation rules.

### Current Implementation

Only supports `gmh` (German Middle High German) from `de` (German).

```csharp
public string Construct(string baseName, string language)
{
    IDictionary<string, string> transformations;

    if (language.Equals("gmh"))
    {
        transformations = germanMiddleHighTransformations;
    }
    else
    {
        transformations = new Dictionary<string, string>();
    }

    return ApplyTransformations(baseName, transformations);
}

public string ApplyTransformations(string name, IDictionary<string, string> transformations)
{
    string transformedName = name;
    foreach (string pattern in transformations.Keys)
    {
        transformedName = Regex.Replace(transformedName, pattern, transformations[pattern]);
    }
    return transformedName;
}
```

### Transformation Rules (gmh)

~70 regex patterns including:
- `irsch` → `ires`
- `sch` → `s`, `Sch` → `S`
- `hl` → `hel`
- `([Bb])erg` → `$1ërc`
- `([Bb])[uü]rg` → `$1urc`
- `land` → `lant`
- `thal` → `tal`
- `ck` → `k`
- `ss` → `z`, `ß` → `z`
- And many more phonetic/orthographic shifts

### Extensibility

To add new language construction:
1. Add entry to `ExonymsService.languagesToConstruct`
2. Add transformation dictionary to `NameConstructor` constructor
3. Add case in `Construct()` method

### Thread Safety

- `germanMiddleHighTransformations` is `readonly` after construction
- `ApplyTransformations` uses local variables
- Safe for concurrent use

## Gatherers

### IGeoNamesGatherer / GeoNamesGatherer

**Files**: `ExonymsAPI/Service/Gatherers/IGeoNamesGatherer.cs`, `GeoNamesGatherer.cs`

#### Purpose

Fetches and parses GeoNames XML response for a given feature ID.

#### Algorithm

```csharp
public async Task<Location> Gather(string geoNamesId)
{
    // 1. Log start
    // 2. FetchLocation(geoNamesId)
    // 3. Log success with DefaultName and Count
    // 4. Return Location
}

private async Task<Location> FetchLocation(string geoNamesId)
{
    // 1. Select random username from hardcoded list
    // 2. HTTP GET to api.geonames.org/get?geonameId={id}&username={username}
    // 3. Parse XML:
    //    - DefaultName = <name> (normalised for "en")
    //    - For each <alternateName lang="...">:
    //      * Skip if lang empty, in IgnoredLanguageCodes, or already in Names
    //      * Create Name with OriginalValue = element.Value
    //      * Transliterate → Normalise
    //      * Add to Names[lang]
    // 4. Return Location
}
```

#### Configuration

- **Endpoint**: `http://api.geonames.org/get?geonameId={0}&username={1}`
- **Default name language**: `en`
- **Ignored language codes**: `link`, `unlc`, `wkdt`
- **Usernames**: 60+ hardcoded accounts (random selection per request)

#### Error Handling

- HTTP non-success → `HttpRequestException`
- XML parsing errors → exception
- All exceptions logged with `OperationStatus.Failure` then rethrown

### IWikiDataGatherer / WikiDataGatherer

**Files**: `ExonymsAPI/Service/Gatherers/IWikiDataGatherer.cs`, `WikiDataGatherer.cs`

#### Purpose

Fetches and parses WikiData JSON EntityData for a given entity ID.

#### Algorithm

```csharp
public async Task<Location> Gather(string wikiDataId)
{
    // 1. Log start
    // 2. FetchLocation(wikiDataId)
    // 3. Log success with DefaultName and Count
    // 4. Return Location
}

private async Task<Location> FetchLocation(string wikiDataId)
{
    // 1. HTTP GET to wikidata.org/wiki/Special:EntityData/{id}.json
    // 2. Parse JSON:
    //    - DefaultName = labels["en"].value (normalised for "en")
    //    - For each label: transliterate → normalise → add to Names[lang]
    //    - For each sitelink:
    //      * Extract lang = key.Replace("(news|quote|source|voyage|wiki)", "")
    //      * Skip if lang already in Names
    //      * Create Name from title, transliterate → normalise
    //      * Skip if equals DefaultName
    //      * Add to Names[lang]
    // 3. Return Location
}
```

#### Configuration

- **Endpoint**: `https://wikidata.org/wiki/Special:EntityData/{id}.json`
- **Default name language**: `en` (constant `WikiDataGatherer.DefaultNameLanguageCode`)
- **Sitelink suffixes removed**: `wiki`, `news`, `quote`, `source`, `voyage`

#### Error Handling

- HTTP non-success → `HttpRequestException`
- JSON parsing errors → exception
- Missing entity → empty Location (no exception)
- All exceptions logged with `OperationStatus.Failure` then rethrown

### Common Gatherer Patterns

| Aspect | GeoNamesGatherer | WikiDataGatherer |
|--------|------------------|------------------|
| Protocol | HTTP GET | HTTP GET |
| Format | XML (LINQ to XML) | JSON (Newtonsoft.Json) |
| Auth | Username (query) | None |
| Client | `new HttpClient()` per call | `HttpClientCreator.Create()` |
| Transliteration | Per alternate name | Per label + sitelink |
| Normalisation | Per alternate name | Per label + sitelink |
| Logging | `MyOperation.GatherGeoNamesExonyms` | `MyOperation.GatherWikiDataExonyms` |

## TransliterationApiClient

**File**: `ExonymsAPI/Client/TransliterationAPI/TransliterationApiClient.cs`  
**Interface**: `ITransliterationApiClient`

### Purpose

Calls external transliteration API to convert names to Latin script.

### Algorithm

```csharp
public async Task<string> Transliterate(string languageCode, string name)
{
    // 1. Check if languageCode in languageCodesToTransliterate (68 languages)
    // 2. If not, return name unchanged
    // 3. Create GetTransliterationsRequest { Text = name, Language = languageCode }
    // 4. Call nuciApiClient.SendRequestAsync<GetTransliterationsRequest, GetTransliterationsResponse>(
    //        HttpMethod.Get, request, "Transliteration")
    // 5. If not successful or response.Text empty, return name
    // 6. Return response.Text
}
```

### Supported Languages (68)

Cyrillic: `ru`, `uk`, `be`, `bg`, `mk`, `sr-ec`, `kk`, `ky`, `mn`, `tg-cyrl`, `tt-cyrl`, `cv`, `ba`, `os`, `udm`  
Arabic: `ar`, `ary`, `arz`  
Other: `el`, `grc`, `grc-dor`, `he`, `hi`, `hy`, `hyw`, `ja`, `ka`, `ko`, `th`, `zh`, `zh-hans`, `bn`, `gu`, `kn`, `ml`, `mr`, `ta`, `te`, `sa`, `iu`, `cop`, `cu`, `sh`, `si`  
Latin with diacritics: `ab`, `ady`, `ba`, `cv`, `sh`, `sr`

### Error Handling

- Language not in list → return original (no API call)
- API error/unsuccessful → return original (graceful degradation)
- Empty response → return original
- No retries, no circuit breaker

### Dependencies

- `INuciApiClient` (transient, created with `TransliterationSettings.TransliterationApiBaseUrl`)
- `NuciApiClient` handles HTTP, serialisation, envelope wrapping

## Service Interaction Diagram

```
ExonymsController.Get()
    │
    ▼
ExonymsService.Gather(geoNamesId, wikiDataId)
    │
    ├──► WikiDataGatherer.Gather(wikiDataId) ──────► HTTP → WikiData
    │       │
    │       ├──► TransliterationApiClient.Transliterate() ───► HTTP → Transliteration API
    │       │
    │       └──► NameNormaliser.Normalise()
    │
    ├──► GeoNamesGatherer.Gather(geoNamesId) ──────► HTTP → GeoNames
    │       │
    │       ├──► TransliterationApiClient.Transliterate() ───► HTTP → Transliteration API
    │       │
    │       └──► NameNormaliser.Normalise()
    │
    ├──► Merge (WikiData first)
    │
    ├──► ConstructNames() ──────► NameConstructor.Construct()
    │
    ├──► ApplyFallbacks() ──────► TransliterationApiClient.Transliterate() (multiple)
    │       └──► NameNormaliser.Normalise()
    │
    ├──► RemoveRedundantExonyms()
    │
    ├──► Sort by language code
    │
    ▼
Location → ExonymsController → GetExonymsResponse → SignHMAC → HTTP Response
```