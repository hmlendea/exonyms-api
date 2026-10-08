# Exonyms API Documentation

## Repository Summary

The Exonyms API is a REST service that gathers exonyms (names of geographical locations in foreign languages) for a given location identifier. It aggregates data from GeoNames and WikiData, applies transliteration to Latin script, normalises names by removing administrative suffixes, constructs historical language variants, and applies language fallbacks for missing exonyms.

## Root Document Map

- [ARCHITECTURE.md](../ARCHITECTURE.md) — High-level architecture
- [SECURITY.md](../SECURITY.md) — Security model and threat mitigations
- [ROADMAP.md](../ROADMAP.md) — Future development plans
- [PRIVACY.md](../PRIVACY.md) — Data handling and privacy
- [LICENSE](../LICENSE) — MIT License

## Documentation Catalogue

### Architecture & Design
- [architecture.md](./architecture.md) — Detailed architecture complementing root ARCHITECTURE.md
- [repository-overview.md](./repository-overview.md) — Purpose, scope, entry points
- [repository-structure.md](./repository-structure.md) — Source tree layout and module organisation
- [design-decisions.md](./design-decisions.md) — Key architectural choices and rationale
- [dependencies.md](./dependencies.md) — External and internal dependencies

### Configuration & Data
- [configuration.md](./configuration.md) — Configuration schema, sources, precedence
- [data-model.md](./data-model.md) — Domain entities, relationships, persistence
- [state-and-persistence.md](./state-and-persistence.md) — State management, caches, migrations

### Components
- [components/presentation.md](./components/presentation.md) — API controllers, request/response models
- [components/host-and-composition.md](./components/host-and-composition.md) — Startup, DI composition, middleware
- [components/application-services.md](./components/application-services.md) — Core business logic services
- [components/integration-models.md](./components/integration-models.md) — External API client models

### Execution Flows
- [flows/startup-and-rendering.md](./flows/startup-and-rendering.md) — Application startup and request pipeline
- [flows/exonym-gathering.md](./flows/exonym-gathering.md) — End-to-end exonym gathering flow

### Behaviours
- [behaviour/browse-and-search.md](./behaviour/browse-and-search.md) — Exonym retrieval behaviour

### Cross-Cutting Concerns
- [integrations.md](./integrations.md) — External system integrations (GeoNames, WikiData, Transliteration API)
- [testing.md](./testing.md) — Test strategy, organisation, coverage
- [concurrency-and-scheduling.md](./concurrency-and-scheduling.md) — Threading, async patterns
- [error-handling.md](./error-handling.md) — Error taxonomy, handling patterns, recovery
- [logging.md](./logging.md) — Logging framework, levels, structured fields, correlation, sinks
- [invariants.md](./invariants.md) — System-wide invariants and contracts
- [build-and-deployment.md](./build-and-deployment.md) — Build pipeline, deployment, environments
- [security.md](./security.md) — Security model (complements root SECURITY.md)
- [ambiguities-and-open-questions.md](./ambiguities-and-open-questions.md) — Unresolved items, TODOs, known gaps
- [change-guide.md](./change-guide.md) — How to modify common areas safely
- [documentation-maintenance.md](./documentation-maintenance.md) — How to keep docs current, ownership

### API Reference
- [api-reference/INDEX.md](./api-reference/INDEX.md) — API reference index, endpoint summary table
- [api-reference/exonyms-controller.md](./api-reference/exonyms-controller.md) — ExonymsController endpoints

## Navigation Aids

- **Start here** (newcomers): [repository-overview.md](./repository-overview.md) → [architecture.md](./architecture.md) → [flows/exonym-gathering.md](./flows/exonym-gathering.md)
- **Deep dive** (component owners): [components/](./components/) → [data-model.md](./data-model.md) → [integrations.md](./integrations.md)
- **Flows** (debuggers): [flows/exonym-gathering.md](./flows/exonym-gathering.md) → [components/application-services.md](./components/application-services.md)

## Maintenance Metadata

- Last reviewed: 2026-10-08
- Owner: hmlendea
- Coverage status: Comprehensive — all major components, flows, and behaviours documented