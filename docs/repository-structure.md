# Repository Structure

## Source Tree Layout

```
exonyms-api/
├── ExonymsAPI.sln                          # Solution file
├── LICENSE                                 # MIT License
├── README.md                               # Project overview, build/run instructions
├── release.sh                              # Release script (delegates to deployment-scripts)
├── .github/workflows/dotnet.yml            # CI/CD pipeline
├── ExonymsAPI/                             # Main API project
│   ├── ExonymsAPI.csproj                   # Project file (net10.0)
│   ├── Program.cs                          # Entry point
│   ├── Startup.cs                          # ASP.NET Core startup
│   ├── ServiceCollectionExtensions.cs      # DI composition
│   ├── appsettings.json                    # Configuration
│   ├── appsettings.Development.json        # Dev overrides
│   ├── API/
│   │   ├── Controllers/
│   │   │   └── ExonymsController.cs        # Single controller
│   │   ├── Requests/
│   │   │   └── GetExonymsRequest.cs        # Query parameters
│   │   └── Responses/
│   │       └── GetExonymsResponse.cs       # Response model
│   ├── Client/
│   │   └── TransliterationAPI/
│   │       ├── ITransliterationApiClient.cs
│   │       ├── TransliterationApiClient.cs
│   │       ├── Requests/
│   │       │   └── GetTransliterationsRequest.cs
│   │       └── Responses/
│   │           └── GetTransliterationsResponse.cs
│   ├── Configuration/
│   │   ├── SecuritySettings.cs
│   │   └── TransliterationSettings.cs
│   ├── Logging/
│   │   ├── MyLogInfoKey.cs                 # Structured log keys
│   │   └── MyOperation.cs                  # Operation names
│   ├── Service/
│   │   ├── IExonymsService.cs
│   │   ├── ExonymsService.cs               # Core orchestration
│   │   ├── Gatherers/
│   │   │   ├── IGeoNamesGatherer.cs
│   │   │   ├── GeoNamesGatherer.cs
│   │   │   ├── IWikiDataGatherer.cs
│   │   │   └── WikiDataGatherer.cs
│   │   ├── Models/
│   │   │   ├── Location.cs
│   │   │   └── Name.cs
│   │   └── Processors/
│   │       ├── INameConstructor.cs
│   │       ├── NameConstructor.cs
│   │       ├── INameNormaliser.cs
│   │       └── NameNormaliser.cs
│   ├── bin/                                # Build output (gitignored)
│   └── obj/                                # Build intermediates (gitignored)
└── ExonymsAPI.UnitTests/                   # Unit test project
    ├── ExonymsAPI.UnitTests.csproj
    ├── Service/
    │   ├── ExonymsServiceTests.cs
    │   ├── NameNormaliserTests.cs
    │   ├── Constructors/
    │   │   └── NameConstructorTests.cs
    │   └── Gatherers/
    │       ├── GeoNamesGathererTests.cs
    │       └── WikiDataGathererTests.cs
    ├── bin/                                # Build output (gitignored)
    └── obj/                                # Build intermediates (gitignored)
```

## Module Organisation

### ExonymsAPI (Main Project)

| Namespace | Purpose | Key Files |
|-----------|---------|-----------|
| `ExonymsAPI` | Host, startup, DI | `Program.cs`, `Startup.cs`, `ServiceCollectionExtensions.cs` |
| `ExonymsAPI.API.Controllers` | HTTP endpoints | `ExonymsController.cs` |
| `ExonymsAPI.API.Requests` | Request DTOs | `GetExonymsRequest.cs` |
| `ExonymsAPI.API.Responses` | Response DTOs | `GetExonymsResponse.cs` |
| `ExonymsAPI.Client.TransliterationAPI` | External API client | `ITransliterationApiClient.cs`, `TransliterationApiClient.cs` |
| `ExonymsAPI.Configuration` | Settings classes | `SecuritySettings.cs`, `TransliterationSettings.cs` |
| `ExonymsAPI.Logging` | Structured logging keys | `MyLogInfoKey.cs`, `MyOperation.cs` |
| `ExonymsAPI.Service` | Business logic | `IExonymsService.cs`, `ExonymsService.cs` |
| `ExonymsAPI.Service.Gatherers` | External data retrieval | `IGeoNamesGatherer.cs`, `GeoNamesGatherer.cs`, `IWikiDataGatherer.cs`, `WikiDataGatherer.cs` |
| `ExonymsAPI.Service.Models` | Domain models | `Location.cs`, `Name.cs` |
| `ExonymsAPI.Service.Processors` | Name transformations | `INameConstructor.cs`, `NameConstructor.cs`, `INameNormaliser.cs`, `NameNormaliser.cs` |

### ExonymsAPI.UnitTests (Test Project)

| Namespace | Purpose | Key Files |
|-----------|---------|-----------|
| `ExonymsAPI.UnitTests.Service` | Service layer tests | `ExonymsServiceTests.cs`, `NameNormaliserTests.cs` |
| `ExonymsAPI.UnitTests.Service.Constructors` | NameConstructor tests | `NameConstructorTests.cs` |
| `ExonymsAPI.UnitTests.Service.Gatherers` | Gatherer tests | `GeoNamesGathererTests.cs`, `WikiDataGathererTests.cs` |

## Project References

```
ExonymsAPI.UnitTests → ExonymsAPI (ProjectReference)
```

## Build Output

- **Framework**: net10.0
- **Output**: `bin/Debug/net10.0/` (executable + deps)
- **Test output**: `ExonymsAPI.UnitTests/bin/Debug/net10.0/`

## Configuration Files

| File | Purpose |
|------|---------|
| `appsettings.json` | Base configuration (transliteration URL, HMAC key, logging) |
| `appsettings.Development.json` | Development overrides (gitignored if secrets) |
| `ExonymsAPI.csproj` | Package references, target framework |
| `ExonymsAPI.UnitTests.csproj` | Test packages, project reference |
| `.github/workflows/dotnet.yml` | CI pipeline (restore, build, test) |