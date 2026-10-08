# Design Decisions

## 1. Dual-Source Aggregation (GeoNames + WikiData)

**Decision**: Fetch from both GeoNames and WikiData, merge results with WikiData taking precedence.

**Rationale**:
- GeoNames: Comprehensive alternate names, structured XML, free tier with multiple usernames
- WikiData: Rich multilingual labels, sitelinks for additional languages, authoritative IDs
- Merging maximises coverage; WikiData precedence reflects higher curation quality

**Trade-offs**:
- Two external dependencies increase failure surface
- Merge logic adds complexity (duplicate handling, precedence rules)
- No caching means repeated requests hit external APIs

## 2. On-Demand Processing (No Caching)

**Decision**: Every request fetches fresh data from external sources.

**Rationale**:
- Simplicity: No cache invalidation, storage, or consistency concerns
- Data freshness: External sources update frequently
- Low request volume expected (public API, niche use case)

**Trade-offs**:
- Latency: 2-3 external HTTP calls per request
- External rate limits: GeoNames free tier has limits
- No resilience to upstream outages

## 3. Transliteration via External API

**Decision**: Delegate script conversion to a dedicated transliteration service.

**Rationale**:
- Transliteration rules are complex and language-specific
- External service (NuciAPI) maintains comprehensive language coverage
- Separation of concerns: Exonyms API focuses on aggregation

**Trade-offs**:
- Additional external dependency
- Network latency for each non-Latin name
- Failure degrades to returning original script

## 4. Language Fallback Synthesis

**Decision**: Generate missing exonyms by transliterating from related languages.

**Rationale**:
- Many languages share exonyms (e.g., Slavic languages often use Russian form)
- Increases response completeness without additional external calls
- Configurable fallback chains per language

**Trade-offs**:
- Synthesised names may be inaccurate
- Additional transliteration calls increase latency
- Fallback chains are manually maintained

## 5. Name Normalisation via Regex Patterns

**Decision**: Use extensive regex patterns to strip administrative/geographic suffixes.

**Rationale**:
- Exonyms should be pure place names, not "City of X" or "X River"
- Patterns cover 50+ languages with language-specific rules
- Deterministic, fast, no ML dependency

**Trade-offs**:
- Pattern maintenance burden (hundreds of regexes)
- False positives/negatives possible
- Language detection not performed (uses source language code)

## 6. Historical Language Construction (gmh from de)

**Decision**: Apply transformation rules to construct Middle High German from modern German.

**Rationale**:
- Specific domain requirement for historical exonyms
- Rule-based approach is transparent and maintainable
- Limited to one language pair currently

**Trade-offs**:
- Rules are heuristic, not linguistically rigorous
- Only supports gmh←de; extensible but not generalised
- No validation of constructed forms

## 7. HMAC Response Signing

**Decision**: Sign all responses with HMAC-SHA256 using a configured key.

**Rationale**:
- Allows consumers to verify response integrity and authenticity
- Lightweight, no PKI infrastructure needed
- NuciSecurity.HMAC provides standard implementation

**Trade-offs**:
- Key management required (rotation, distribution)
- Adds computation per response
- Public endpoint means key exposure risk if not rotated

## 8. Structured Logging with NuciLog

**Decision**: Use NuciLog for structured, correlated logging with custom operations and keys.

**Rationale**:
- Consistent with organisational logging standards
- Operation/LogInfoKey types enable type-safe structured logging
- File + console output with configurable levels

**Trade-offs**:
- Additional dependency
- Custom types (MyOperation, MyLogInfoKey) require maintenance
- Learning curve for team unfamiliar with NuciLog

## 9. Singleton Services for Gatherers/Processors

**Decision**: Register `IGeoNamesGatherer`, `IWikiDataGatherer`, `INameNormaliser`, `INameConstructor`, `IExonymsService` as singletons.

**Rationale**:
- Stateless implementations (no per-request state)
- HttpClient created per-call internally (GeoNamesGatherer, WikiDataGatherer)
- TransliterationApiClient is transient (wraps NuciApiClient)

**Trade-offs**:
- Must ensure thread-safety (all implementations are stateless)
- TransliterationApiClient transient due to NuciApiClient lifetime

## 10. Random GeoNames Username Selection

**Decision**: Rotate through a hardcoded list of GeoNames usernames.

**Rationale**:
- Free tier rate limits per username
- Rotation distributes load across accounts
- No authentication complexity

**Trade-offs**:
- Hardcoded credentials in source (security concern)
- Account sharing may violate GeoNames ToS
- No fallback if all accounts exhausted

## 11. WikiData Sitelink Language Extraction

**Decision**: Extract language codes from sitelink keys by removing suffixes (wiki, news, quote, source, voyage).

**Rationale**:
- Sitelinks provide additional language coverage beyond labels
- Suffix pattern is consistent across WikiData
- Simple regex extraction

**Trade-offs**:
- Heuristic: not all sitelinks follow pattern
- May extract incorrect language codes
- Duplicates with labels handled by dictionary key check

## 12. Alphabetical Response Ordering

**Decision**: Sort response names by language code before returning.

**Rationale**:
- Deterministic output for testing and caching
- Predictable ordering for consumers
- Simple implementation

**Trade-offs**:
- No semantic ordering (e.g., by language family or prevalence)
- Minor CPU cost (negligible)