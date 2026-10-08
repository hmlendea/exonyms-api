# Exonym Gathering Flow

## End-to-End Flow Overview

```
GET /Exonyms?geoNamesId=310350&wikiDataId=Q310350
    │
    ▼
ExonymsController.Get(request)
    │
    ▼
ExonymsService.Gather(geoNamesId, wikiDataId)
    │
    ├── WikiDataGatherer.Gather(wikiDataId) → Location
    │
    ├── GeoNamesGatherer.Gather(geoNamesId) → Location
    │
    ├── MergeLocations(wikiDataLocation, geoNamesLocation)
    │       └── WikiData takes precedence
    │
    ├── ConstructNames(mergedLocation)
    │       └── Add gmh from de
    │
    ├── ApplyFallbacks(mergedLocation)
    │       └── 13 language groups with fallbacks
    │
    ├── RemoveRedundantExonyms(mergedLocation)
    │       └── Remove duplicates of DefaultName
    │
    ├── SortNamesAlphabetically(mergedLocation)
    │
    ▼
Return Location → Map to GetExonymsResponse → Sign HMAC → Return
```

## Step 1: WikiData Gathering

### WikiDataGatherer.Gather(wikiDataId)

```csharp
public async Task<Location> Gather(string wikiDataId)
{
    // 1. HTTP GET to WikiData
    string url = $"https://wikidata.org/wiki/Special:EntityData/{wikiDataId}.json";
    string json = await httpClient.GetStringAsync(url);

    // 2. Parse JSON
    JObject data = JObject.Parse(json);
    JObject entity = (JObject)data["entities"][wikiDataId];

    Location location = new();

    // 3. Extract default name (English label)
    if (entity["labels"]?["en"]?["value"] is JValue defaultLabel)
    {
        location.DefaultName = nameNormaliser.Normalise("en", (string)defaultLabel);
    }

    // 4. Process all labels
    if (entity["labels"] is JObject labels)
    {
        foreach (var label in labels)
        {
            string lang = label.Key;
            string value = (string)label.Value["value"];

            Name name = new(value);
            name.Value = await transliterationApiClient.Transliterate(lang, value);
            name.Value = nameNormaliser.Normalise(lang, name.Value);
            location.Names[lang] = name;
        }
    }

    // 5. Process sitelinks
    if (entity["sitelinks"] is JObject sitelinks)
    {
        foreach (var sitelink in sitelinks)
        {
            string lang = Regex.Replace(sitelink.Key, @"(news|quote|source|voyage|wiki)", "");

            if (location.Names.ContainsKey(lang))
                continue;

            string value = (string)sitelink.Value["title"];
            Name name = new(value);
            name.Value = await transliterationApiClient.Transliterate(lang, value);
            name.Value = nameNormaliser.Normalise(lang, name.Value);

            if (name.Equals(location.DefaultName))
                continue;

            location.Names[lang] = name;
        }
    }

    return location;
}
```

### WikiData Output Example

```json
{
  "DefaultName": "Al Hoceima",
  "Names": {
    "en": { "OriginalValue": "Al Hoceima", "Value": "Al Hoceima" },
    "ar": { "OriginalValue": "الحسيمة", "Value": "Al Hoceima" },
    "de": { "OriginalValue": "Alhucemas", "Value": "Alhucemas" },
    "fr": { "OriginalValue": "Al Hoceïma", "Value": "Al Hoceima" },
    "ru": { "OriginalValue": "Аль-Хосейма", "Value": "Al-Khoseyma" }
  }
}
```

## Step 2: GeoNames Gathering

### GeoNamesGatherer.Gather(geoNamesId)

```csharp
public async Task<Location> Gather(int geoNamesId)
{
    // 1. Select random username
    string username = usernames.GetRandomElement();

    // 2. HTTP GET to GeoNames
    string url = $"http://api.geonames.org/get?geonameId={geoNamesId}&username={username}";
    string xml = await httpClient.GetStringAsync(url);

    // 3. Parse XML
    XDocument doc = XDocument.Parse(xml);
    XElement geoname = doc.Root;

    Location location = new();

    // 4. Default name
    location.DefaultName = nameNormaliser.Normalise("en", (string)geoname.Element("name"));

    // 5. Process alternate names
    foreach (XElement altName in geoname.Elements("alternateName"))
    {
        string lang = altName.Attribute("lang")?.Value;

        if (string.IsNullOrWhiteSpace(lang) ||
            location.Names.ContainsKey(lang) ||
        IgnoredLanguageCodes.Contains(lang))
        {
            continue;
        }

        Name name = new(altName.Value);
        name.Value = await transliterationApiClient.Transliterate(lang, name.Value);
        name.Value = nameNormaliser.Normalise(lang, name.Value);
        location.Names[lang] = name;
    }

    return location;
}
```

### GeoNames Output Example

```json
{
  "DefaultName": "Al Hoceima",
  "Names": {
    "ar": { "OriginalValue": "الحسيمة", "Value": "Al Hoceima" },
    "de": { "OriginalValue": "Alhucemas", "Value": "Alhucemas" },
    "fr": { "OriginalValue": "Al Hoceïma", "Value": "Al Hoceima" },
    "es": { "OriginalValue": "Alhucemas", "Value": "Alhucemas" },
    "it": { "OriginalValue": "Al Hoceima", "Value": "Al Hoceima" }
  }
}
```

## Step 3: Merge Locations

### MergeLocations(wikiDataLocation, geoNamesLocation)

```csharp
private Location MergeLocations(Location wikiData, Location geoNames)
{
    Location merged = new()
    {
        DefaultName = wikiData.DefaultName ?? geoNames.DefaultName
    };

    // WikiData names take precedence
    foreach (var kvp in wikiData.Names)
    {
        merged.Names[kvp.Key] = kvp.Value;
    }

    // GeoNames names only for missing languages
    foreach (var kvp in geoNames.Names)
    {
        if (!merged.Names.ContainsKey(kvp.Key))
        {
            merged.Names[kvp.Key] = kvp.Value;
        }
    }

    return merged;
}
```

### Merge Rules

| Scenario | Result |
|----------|--------|
| WikiData has lang, GeoNames has lang | WikiData wins |
| WikiData has lang, GeoNames missing | WikiData used |
| WikiData missing, GeoNames has lang | GeoNames used |
| Both missing | Not in result |

### Merged Example

```json
{
  "DefaultName": "Al Hoceima",
  "Names": {
    "en": { "OriginalValue": "Al Hoceima", "Value": "Al Hoceima" },  // WikiData
    "ar": { "OriginalValue": "الحسيمة", "Value": "Al Hoceima" },     // WikiData
    "de": { "OriginalValue": "Alhucemas", "Value": "Alhucemas" },    // WikiData
    "fr": { "OriginalValue": "Al Hoceïma", "Value": "Al Hoceima" },  // WikiData
    "ru": { "OriginalValue": "Аль-Хосейма", "Value": "Al-Khoseyma" }, // WikiData
    "es": { "OriginalValue": "Alhucemas", "Value": "Alhucemas" },    // GeoNames
    "it": { "OriginalValue": "Al Hoceima", "Value": "Al Hoceima" }   // GeoNames
  }
}
```

## Step 4: Construct Names

### ConstructNames(location)

```csharp
private void ConstructNames(Location location)
{
    foreach (var kvp in languagesToConstruct)
    {
        string targetLang = kvp.Key;
        string[] baseLangs = kvp.Value;

        if (location.Names.ContainsKey(targetLang))
            continue;

        foreach (string baseLang in baseLangs)
        {
            if (location.Names.TryGetValue(baseLang, out Name baseName))
            {
                string constructed = nameConstructor.Construct(baseName.Value, targetLang);

                Name name = new(baseName.OriginalValue)
                {
                    Value = constructed,
                    Comment = $"Constructed. Based on language '{baseLang}'"
                };

                location.Names[targetLang] = name;
                break;
            }
        }
    }
}
```

### Construction Mapping

```csharp
private readonly Dictionary<string, string[]> languagesToConstruct = new()
{
    { "gmh", ["de"] }  // German Middle High German from German
};
```

### Construction Example

```
Base: de → "Alhucemas"
    │
    ▼
NameConstructor.Construct("Alhucemas", "gmh")
    │
    ├── Apply 70+ gmh transformation rules
    │   ├── "c" → "k" before e/i
    │   ├── "z" → "tz"
    │   ├── etc.
    │
    ▼
Result: "Alhukemas" (hypothetical)
    │
    ▼
Add to Names:
{
    "gmh": {
        "OriginalValue": "Alhucemas",
        "Value": "Alhukemas",
        "Comment": "Constructed. Based on language 'de'"
    }
}
```

## Step 5: Apply Fallbacks

### ApplyFallbacks(location)

```csharp
private void ApplyFallbacks(Location location)
{
    foreach (var kvp in languageFallbacks)
    {
        string targetLang = kvp.Key;
        string[] fallbackLangs = kvp.Value;

        if (location.Names.ContainsKey(targetLang))
            continue;

        foreach (string fallbackLang in fallbackLangs)
        {
            if (location.Names.TryGetValue(fallbackLang, out Name fallbackName))
            {
                string transliterated = transliterationApiClient
                    .Transliterate(targetLang, fallbackName.OriginalValue).Result;

                // If transliteration didn't change script, try from fallback lang
                if (transliterated == fallbackName.OriginalValue)
                {
                    transliterated = transliterationApiClient
                        .Transliterate(fallbackLang, fallbackName.OriginalValue).Result;
                }

                string normalised = nameNormaliser.Normalise(fallbackLang, transliterated);

                Name name = new(fallbackName.OriginalValue)
                {
                    Value = normalised,
                    Comment = $"Based on language '{fallbackLang}'"
                };

                location.Names[targetLang] = name;
                break;
            }
        }
    }
}
```

### Fallback Mappings (13 groups)

```csharp
private readonly Dictionary<string, string[]> languageFallbacks = new()
{
    { "ab", ["ru"] },           // Abkhazian → Russian
    { "av", ["ru"] },           // Avaric → Russian
    { "ba", ["ru"] },           // Bashkir → Russian
    { "be", ["ru"] },           // Belarusian → Russian
    { "cv", ["ru"] },           // Chuvash → Russian
    { "inh", ["ru"] },          // Ingush → Russian
    { "kbd", ["ru"] },          // Kabardian → Russian
    { "kk", ["ru"] },           // Kazakh → Russian
    { "ky", ["ru"] },           // Kyrgyz → Russian
    { "myv", ["ru"] },          // Erzya → Russian
    { "os", ["ru"] },           // Ossetian → Russian
    { "sah", ["ru"] },          // Yakut → Russian
    { "tt", ["ru"] }            // Tatar → Russian
};
```

### Fallback Example

```
Target: "tt" (Tatar) - not in Names
    │
    ▼
Fallback: "ru" (Russian) - exists in Names
    │
    ▼
fallbackName.OriginalValue = "Аль-Хосейма"
    │
    ▼
Transliterate("tt", "Аль-Хосейма")
    │
    ├── If result ≠ "Аль-Хосейма" → use result
    └── If result = "Аль-Хосейма" (Cyrillic unchanged)
        → Transliterate("ru", "Аль-Хосейма") → "Al-Khoseyma"
    │
    ▼
Normalise("ru", "Al-Khoseyma") → "Al-Khoseyma"
    │
    ▼
Add to Names:
{
    "tt": {
        "OriginalValue": "Аль-Хосейма",
        "Value": "Al-Khoseyma",
        "Comment": "Based on language 'ru'"
    }
}
```

## Step 6: Remove Redundant Exonyms

### RemoveRedundantExonyms(location)

```csharp
private void RemoveRedundantExonyms(Location location)
{
    List<string> keysToRemove = location.Names
        .Where(kvp => kvp.Value.Value.Equals(location.DefaultName, StringComparison.OrdinalIgnoreCase))
        .Select(kvp => kvp.Key)
        .ToList();

    foreach (string key in keysToRemove)
    {
        location.Names.Remove(key);
    }
}
```

### Redundancy Example

```
DefaultName: "Al Hoceima"
Names:
  "en": "Al Hoceima"     → REMOVE (matches DefaultName)
  "fr": "Al Hoceima"     → REMOVE (matches DefaultName)
  "de": "Alhucemas"      → KEEP
  "ar": "Al Hoceima"     → REMOVE (matches DefaultName)
  "ru": "Al-Khoseyma"    → KEEP
```

## Step 7: Sort Names Alphabetically

### SortNamesAlphabetically(location)

```csharp
private void SortNamesAlphabetically(Location location)
{
    location.Names = location.Names
        .OrderBy(kvp => kvp.Key)
        .ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
}
```

### Final Sorted Output

```json
{
  "DefaultName": "Al Hoceima",
  "Names": {
    "ar": { "OriginalValue": "الحسيمة", "Value": "Al Hoceima" },
    "de": { "OriginalValue": "Alhucemas", "Value": "Alhucemas" },
    "es": { "OriginalValue": "Alhucemas", "Value": "Alhucemas" },
    "gmh": { "OriginalValue": "Alhucemas", "Value": "Alhukemas", "Comment": "Constructed. Based on language 'de'" },
    "it": { "OriginalValue": "Al Hoceima", "Value": "Al Hoceima" },
    "ru": { "OriginalValue": "Аль-Хосейма", "Value": "Al-Khoseyma" },
    "tt": { "OriginalValue": "Аль-Хосейма", "Value": "Al-Khoseyma", "Comment": "Based on language 'ru'" }
  }
}
```

## Complete Flow Diagram

```
┌─────────────────────────────────────────────────────────────────┐
│                    ExonymsService.Gather                        │
└─────────────────────────────────────────────────────────────────┘
                              │
              ┌───────────────┴───────────────┐
              ▼                               ▼
    ┌─────────────────────┐         ┌─────────────────────┐
    │ WikiDataGatherer    │         │ GeoNamesGatherer    │
    │ .Gather(wikiDataId) │         │ .Gather(geoNamesId) │
    └─────────────────────┘         └─────────────────────┘
              │                               │
              ▼                               ▼
    ┌─────────────────────┐         ┌─────────────────────┐
    │ Location (WikiData) │         │ Location (GeoNames) │
    │ - DefaultName       │         │ - DefaultName       │
    │ - Names[lang]       │         │ - Names[lang]       │
    └─────────────────────┘         └─────────────────────┘
              │                               │
              └───────────────┬───────────────┘
                              ▼
              ┌─────────────────────────────┐
              │ MergeLocations              │
              │ WikiData precedence         │
              └─────────────────────────────┘
                              │
                              ▼
              ┌─────────────────────────────┐
              │ ConstructNames              │
              │ gmh from de                 │
              └─────────────────────────────┘
                              │
                              ▼
              ┌─────────────────────────────┐
              │ ApplyFallbacks              │
              │ 13 language groups → ru     │
              └─────────────────────────────┘
                              │
                              ▼
              ┌─────────────────────────────┐
              │ RemoveRedundantExonyms      │
              │ Remove = DefaultName        │
              └─────────────────────────────┘
                              │
                              ▼
              ┌─────────────────────────────┐
              │ SortNamesAlphabetically     │
              │ By language code            │
              └─────────────────────────────┘
                              │
                              ▼
                    ┌─────────────────────┐
                    │ Location (final)    │
                    └─────────────────────┘
```

## Error Handling in Flow

| Step | Failure Mode | Behaviour |
|------|--------------|-----------|
| WikiData HTTP | Network/4xx/5xx | Exception → 500 |
| WikiData JSON parse | Malformed | Exception → 500 |
| WikiData missing entity | 404/empty | Empty Location → merge continues |
| GeoNames HTTP | Network/4xx/5xx | Exception → 500 |
| GeoNames XML parse | Malformed | Exception → 500 |
| Transliteration | Any error | Return original, continue |
| Normalisation | Regex error | Exception → 500 |
| Construction | Regex error | Exception → 500 |
| Fallback | Transliteration error | Return original, continue |

## Performance Characteristics

| Operation | Complexity | Notes |
|-----------|------------|-------|
| WikiData HTTP | O(1) network | ~200-500ms |
| GeoNames HTTP | O(1) network | ~100-300ms |
| Merge | O(n) | n = total names |
| Construct | O(1) | Single rule set (gmh) |
| Fallbacks | O(m×k) | m=13 groups, k=fallbacks |
| Remove redundant | O(n) | String comparison |
| Sort | O(n log n) | n = final name count |

**Total typical latency**: 300-800ms (dominated by external HTTP calls)

## Concurrency

- `ExonymsService` is **Singleton**
- `Gather` is **not thread-safe** for same instance
- Multiple concurrent requests share same service instance
- No locking → potential race conditions on `Location.Names` dictionary
- **Mitigation**: Each request creates new `Location` objects (no shared mutable state)
- `HttpClient` instances created per gatherer call (not pooled) — consider `IHttpClientFactory`

## Caching

**None implemented**. Every request:
1. Calls WikiData API
2. Calls GeoNames API
3. Calls Transliteration API for each name
4. Runs all transformations

**Opportunity**: Cache merged results by `(geoNamesId, wikiDataId)` key.