# Architecture

This document complements the root [ARCHITECTURE.md](../ARCHITECTURE.md) with implementation-level detail.

## High-Level Architecture

```
┌─────────────────────────────────────────────────────────────────┐
│                        ExonymsAPI (ASP.NET Core)                │
├─────────────────────────────────────────────────────────────────┤
│  Controllers                                                    │
│  └── ExonymsController                                          │
│       └── GET /Exonyms?geoNamesId=...&wikiDataId=...           │
├─────────────────────────────────────────────────────────────────┤
│  Application Services                                           │
│  └── ExonymsService (IExonymsService)                          │
│       ├── Gather(geoNamesId, wikiDataId) → Location            │
│       ├── ConstructNames()                                     │
│       ├── ApplyFallbacks()                                     │
│       └── RemoveRedundantExonyms()                             │
├─────────────────────────────────────────────────────────────────┤
│  Gatherers (External Data Sources)                              │
│  ├── IGeoNamesGatherer → GeoNamesGatherer                      │
│  │     └── HTTP → api.geonames.org (XML)                       │
│  └── IWikiDataGatherer → WikiDataGatherer                      │
│        └── HTTP → wikidata.org (JSON)                          │
├─────────────────────────────────────────────────────────────────┤
│  Processors                                                     │
│  ├── INameNormaliser → NameNormaliser                          │
│  │     └── Regex-based suffix/prefix removal                   │
│  └── INameConstructor → NameConstructor                        │
│        └── German Middle High German transformations           │
├─────────────────────────────────────────────────────────────────┤
│  External Clients                                               │
│  └── ITransliterationApiClient → TransliterationApiClient      │
│        └── HTTP → Transliteration API (NuciAPI)                │
└─────────────────────────────────────────────────────────────────┘
```

## Architectural Layers

| Layer | Responsibility | Key Types |
|-------|---------------|-----------|
| Presentation | HTTP endpoint, request validation, response signing | `ExonymsController`, `GetExonymsRequest`, `GetExonymsResponse` |
| Application Services | Orchestration, business rules, merging, fallbacks | `ExonymsService`, `IExonymsService` |
| Gatherers | External data retrieval, parsing, normalisation | `IGeoNamesGatherer`, `IWikiDataGatherer`, `GeoNamesGatherer`, `WikiDataGatherer` |
| Processors | Name transformation, construction, normalisation | `INameNormaliser`, `INameConstructor`, `NameNormaliser`, `NameConstructor` |
| External Clients | Third-party API communication | `ITransliterationApiClient`, `TransliterationApiClient` |
| Infrastructure | DI, logging, configuration, middleware | `ServiceCollectionExtensions`, `Startup`, `Program` |

## Dependency Direction

```
ExonymsController
    └── IExonymsService (ExonymsService)
            ├── IGeoNamesGatherer (GeoNamesGatherer)
            │     ├── INameNormaliser (NameNormaliser)
            │     └── ITransliterationApiClient (TransliterationApiClient)
            ├── IWikiDataGatherer (WikiDataGatherer)
            │     ├── INameNormaliser (NameNormaliser)
            │     └── ITransliterationApiClient (TransliterationApiClient)
            ├── INameConstructor (NameConstructor)
            ├── INameNormaliser (NameNormaliser)
            └── ITransliterationApiClient (TransliterationApiClient)
```

All dependencies flow inward toward domain logic. External clients are abstracted behind interfaces.

## Runtime Topology

- **Single process**: ASP.NET Core Kestrel host
- **Stateless**: No in-process state between requests
- **External dependencies**: GeoNames (HTTP/XML), WikiData (HTTP/JSON), Transliteration API (HTTP/JSON)
- **Concurrency**: Thread-per-request (ASP.NET Core default), async/await throughout

## Major State

No persistent state in the application. All data is fetched on-demand from external sources.

## System-Wide Invariants

1. **Default name precedence**: WikiData default name takes precedence over GeoNames when both provided
2. **Language uniqueness**: Each language code appears at most once in the result
3. **Alphabetical ordering**: Response names are sorted by language code
4. **Latin script**: All returned names are transliterated to Latin script
5. **No redundant exonyms**: Exonyms identical to the default name (except for `en`) are removed
6. **HMAC signing**: All responses are HMAC-signed with the configured key