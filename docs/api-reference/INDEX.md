# API Reference Index

## Overview

This section provides comprehensive documentation for the Exonyms API, including endpoint specifications, request/response formats, and usage examples. The API is designed to provide exonyms (alternative names for places in different languages) by aggregating data from multiple sources.

## Navigation

- [Exonyms Controller](exonyms-controller.md) - Main endpoint documentation
- [Error Models](error-models.md) - API error response formats
- [Response Models](response-models.md) - Data structure definitions

## API Version

Current version: **v1** (stable)

## Base URL

```
https://api.example.com
```

## Authentication

The API uses HMAC-SHA256 signature verification for response integrity. No authentication is required for requests.

## Rate Limits

- **No server-side rate limiting** implemented
- External dependencies have their own limits:
  - GeoNames: ~2000 requests/hour per username
  - WikiData: No strict limits
  - Transliteration API: Service-specific limits

## Response Format

All successful responses follow this structure:

```json
{
  "success": true,
  "defaultName": "Al Hoceima",
  "names": {
    "ar": {
      "originalValue": "الحسيمة",
      "value": "Al Hoceima",
      "comment": null
    }
  },
  "count": 1,
  "hmac": "sha256=abc123..."
}
```

## Error Handling

Errors are returned as JSON with the same structure but `success: false`:

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

## Documentation Structure

This API reference is organized to help developers understand:

1. **Endpoint Usage** - How to call the API and what to expect
2. **Data Models** - Complete definitions of all request/response types
3. **Error Conditions** - All possible error scenarios and their meanings

## Related Documentation

- [Architecture Overview](https://github.com/username/exonyms-api/blob/main/ARCHITECTURE.md) - High-level system design
- [Repository Structure](https://github.com/username/exonyms-api/blob/main/docs/repository-structure.md) - Code organization
- [Design Decisions](https://github.com/username/exonyms-api/blob/main/docs/design-decisions.md) - Rationale behind architectural choices
- [Dependencies](https://github.com/username/exonyms-api/blob/main/docs/dependencies.md) - External libraries and services
- [Configuration](https://github.com/username/exonyms-api/blob/main/docs/configuration.md) - Settings and environment setup
- [Data Model](https://github.com/username/exonyms-api/blob/main/docs/data-model.md) - Domain entity definitions
- [State and Persistence](https://github.com/username/exonyms-api/blob/main/docs/state-and-persistence.md) - Data storage and caching
- [Components](https://github.com/username/exonyms-api/blob/main/docs/components/) - System component documentation
- [Flows](https://github.com/username/exonyms-api/blob/main/docs/flows/) - Data flow and processing pipelines
- [Behavior](https://github.com/username/exonyms-api/blob/main/docs/behaviour/) - User-facing behavior and usage patterns
- [Integrations](https://github.com/username/exonyms-api/blob/main/docs/integrations.md) - External system integrations
- [Testing](https://github.com/username/exonyms-api/blob/main/docs/testing.md) - Testing strategy and coverage
- [Concurrency and Scheduling](https://github.com/username/exonyms-api/blob/main/docs/concurrency-and-scheduling.md) - Parallel processing and scheduling
- [Error Handling](https://github.com/username/exonyms-api/blob/main/docs/error-handling.md) - Error handling patterns
- [Logging](https://github.com/username/exonyms-api/blob/main/docs/logging.md) - Logging and monitoring
- [Invariants](https://github.com/username/exonyms-api/blob/main/docs/invariants.md) - System invariants and guarantees
- [Build and Deployment](https://github.com/username/exonyms-api/blob/main/docs/build-and-deployment.md) - CI/CD and deployment
- [Security](https://github.com/username/exonyms-api/blob/main/docs/security.md) - Security considerations
- [Ambiguities and Open Questions](https://github.com/username/exonyms-api/blob/main/docs/ambiguities-and-open-questions.md) - Known limitations and TODOs
- [Change Guide](https://github.com/username/exonyms-api/blob/main/docs/change-guide.md) - Modification and upgrade procedures
- [Documentation Maintenance](https://github.com/username/exonyms-api/blob/main/docs/documentation-maintenance.md) - Documentation update guidelines

## Contact

For questions about the API, please refer to the main repository documentation or contact the maintainers.