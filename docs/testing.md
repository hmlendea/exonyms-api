# Testing

## Overview

The Exonyms API uses a comprehensive unit testing strategy with **NUnit** as the test framework and **Moq** for mocking dependencies. Tests cover all service-layer components including the core `ExonymsService`, name processors, and data gatherers.

## Test Project Structure

```
ExonymsAPI.UnitTests/
├── ExonymsAPI.UnitTests.csproj
├── Service/
│   ├── ExonymsServiceTests.cs
│   ├── NameNormaliserTests.cs
│   └── Constructors/
│       └── NameConstructorTests.cs
└── Gatherers/
    ├── GeoNamesGathererTests.cs
    └── WikiDataGathererTests.cs
```

## Test Framework

| Component | Version | Purpose |
|-----------|---------|---------|
| NUnit | 3.x | Test framework |
| Moq | 4.x | Mocking framework |
| FluentAssertions | 6.x | Assertion library |

## Test Categories

### 1. Service Layer Tests

#### ExonymsServiceTests

**File**: `ExonymsAPI.UnitTests/Service/ExonymsServiceTests.cs`

**Coverage**:
- `Gather()` method orchestration
- Merge logic (WikiData precedence)
- Construction (gmh from de)
- Fallbacks (13 language groups)
- Deduplication
- Sorting

**Key Test Cases**:
- Merging with WikiData precedence
- Construction of gmh names
- Fallback synthesis for 13 language groups
- Removal of redundant exonyms
- Alphabetical sorting by language code

**Mocking Strategy**:
```csharp
var wikiDataGatherer = new Mock<IWikiDataGatherer>();
var geoNamesGatherer = new Mock<IGeoNamesGatherer>();
var transliterationApiClient = new Mock<ITransliterationApiClient>();
var nameNormaliser = new Mock<INameNormaliser>();
var nameConstructor = new Mock<INameConstructor>();

var service = new ExonymsService(
    wikiDataGatherer.Object,
    geoNamesGatherer.Object,
    transliterationApiClient.Object,
    nameNormaliser.Object,
    nameConstructor.Object);
```

#### NameNormaliserTests

**File**: `ExonymsAPI.UnitTests/Service/NameNormaliserTests.cs`

**Coverage**:
- Generic pattern removal (suffixes, prefixes, quotes, XML tags)
- Language-specific normalisation (50+ languages)
- Category-specific patterns (City, County, River, etc.)
- Whitespace and dash cleanup

**Key Test Cases**:
- Removal of administrative suffixes
- Normalisation of "of" words in multiple languages
- Handling of empty/null inputs
- Whitespace trimming

#### NameConstructorTests

**File**: `ExonymsAPI.UnitTests/Service/Constructors/NameConstructorTests.cs`

**Coverage**:
- German Middle High German (gmh) construction from German (de)
- 70+ transformation rules

**Key Test Cases**:
- Basic gmh transformations
- Edge cases in rule application
- Preservation of non-applicable characters

### 2. Gatherer Tests

#### GeoNamesGathererTests

**File**: `ExonymsAPI.UnitTests/Gatherers/GeoNamesGathererTests.cs`

**Coverage**:
- XML parsing
- Language code filtering
- Username rotation
- Name processing pipeline

**Key Test Cases**:
- Parsing valid GeoNames XML
- Filtering ignored language codes (`link`, `unlc`, `wkdt`)
- Handling missing alternate names
- Transliteration integration

#### WikiDataGathererTests

**File**: `ExonymsAPI.UnitTests/Gatherers/WikiDataGathererTests.cs`

**Coverage**:
- JSON parsing
- Label extraction
- Sitelink parsing
- Language code extraction from sitelinks

**Key Test Cases**:
- Parsing valid WikiData JSON
- Extracting labels in all languages
- Extracting language codes from sitelinks
- Handling missing entities
- Handling missing labels/sitelinks

## Test Data

### Standard Test Values

Tests use consistent test data following standard conventions:

| Entity | GeoNames ID | WikiData ID | Default Name |
|--------|-------------|-------------|--------------|
| Al Hoceima | 310350 | Q310350 | Al Hoceima |
| Moscow | 435181 | Q123456 | Moscow |
| Berlin | 2950159 | Q64 | Berlin |

### Language Codes Used in Tests

- `en` (English)
- `de` (German)
- `fr` (French)
- `ar` (Arabic)
- `ru` (Russian)
- `es` (Spanish)
- `gmh` (German Middle High German)

## Test Execution

### Running Tests

```bash
# From repository root
dotnet test

# From test project directory
cd ExonymsAPI.UnitTests
dotnet test

# With verbose output
dotnet test --verbosity normal

# Filter by test name
dotnet test --filter "FullyQualifiedName~ExonymsService"

# Filter by category
dotnet test --filter "Category=Unit"
```

### CI Integration

**File**: `.github/workflows/dotnet.yml`

```yaml
- name: Test
  run: dotnet test --no-restore --verbosity normal
```

Tests run as part of CI pipeline on every push and pull request.

## Mocking Strategy

### Interfaces Mocked

| Interface | Purpose | Mocked In |
|-----------|---------|-----------|
| `IExonymsService` | Core service | Controller tests (if any) |
| `IGeoNamesGatherer` | GeoNames data gathering | Service tests |
| `IWikiDataGatherer` | WikiData data gathering | Service tests |
| `ITransliterationApiClient` | Transliteration API | Gatherer tests, Service tests |
| `INameNormaliser` | Name normalisation | Gatherer tests, Service tests |
| `INameConstructor` | Name construction | Service tests |

### Mock Patterns

```csharp
// Setup return value
wikiDataGatherer
    .Setup(x => x.Gather(It.IsAny<string>()))
    .ReturnsAsync(location);

// Setup with specific parameter
geoNamesGatherer
    .Setup(x => x.Gather(It.Is<int>(id => id == 310350)))
    .ReturnsAsync(location);

// Verify call
transliterationApiClient
    .Verify(x => x.Transliterate("ru", "Москва"), Times.Once);
```

## Test Coverage

### Covered Components

| Component | Coverage | Notes |
|-----------|----------|-------|
| ExonymsService | High | All pipeline steps tested |
| NameNormaliser | High | Regex patterns tested |
| NameConstructor | High | gmh rules tested |
| GeoNamesGatherer | Medium | XML parsing tested |
| WikiDataGatherer | Medium | JSON parsing tested |
| ExonymsController | Low | No direct controller tests |

### Coverage Gaps

| Component | Gap | Recommendation |
|-----------|-----|----------------|
| ExonymsController | No tests | Add controller tests with mocked service |
| HMAC signing | Not tested | Add tests for response signing |
| Middleware | Not tested | Add integration tests for pipeline |
| External APIs | Not tested | Add integration tests with HTTP mocks |
| Error handling | Partial | Add tests for exception scenarios |

## Integration Testing

### Current State

**No integration tests** exist. All tests are unit tests with mocked dependencies.

### Recommended Integration Tests

1. **Full pipeline test**
   - Mock external HTTP responses
   - Test end-to-end flow
   - Verify response format

2. **External API contract tests**
   - Verify GeoNames XML parsing
   - Verify WikiData JSON parsing
   - Verify Transliteration API envelope

3. **HMAC verification tests**
   - Test signing and verification
   - Test with invalid keys

4. **Middleware tests**
   - Test request logging
   - Test exception handling
   - Test HTTPS redirection

### Integration Test Setup

```csharp
// Example: Using WebApplicationFactory
public class ExonymsControllerIntegrationTests : IClassFixture<WebApplicationFactory<Startup>>
{
    private readonly HttpClient _client;

    public ExonymsControllerIntegrationTests(WebApplicationFactory<Startup> factory)
    {
        _client = factory.CreateClient();
    }

    [Test]
    public async Task Get_WithValidIds_ReturnsExonyms()
    {
        var response = await _client.GetAsync("/Exonyms?geoNamesId=310350&wikiDataId=Q310350");
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }
}
```

## Test Quality Standards

### Naming Convention

```
MethodName_StateUnderTest_ExpectedBehavior
```

**Examples**:
- `Gather_WithBothIds_WikiDataTakesPrecedence`
- `Gather_WithMissingGeoNamesId_UsesWikiDataOnly`
- `Normalise_WithAdministrativeSuffix_RemovesSuffix`

### Assertion Style

Uses **FluentAssertions** for readable assertions:

```csharp
result.DefaultName.Should().Be("Al Hoceima");
result.Names.Should().ContainKey("de");
result.Names["de"].Value.Should().Be("Alhucemas");
result.Names["gmh"].Comment.Should().Contain("Constructed");
```

### Test Isolation

- Each test creates fresh mocks
- No shared state between tests
- Tests are independent and order-independent

## Continuous Integration

### CI Pipeline

**File**: `.github/workflows/dotnet.yml`

```yaml
jobs:
  build:
    runs-on: ubuntu-latest
    steps:
      - Checkout
      - Setup .NET
      - Restore
      - Build
      - Test
      - (Optional) Publish
```

### Quality Gates

- All tests must pass
- No test failures allowed in CI
- Code coverage threshold (if configured)

## Test Maintenance

### Adding New Tests

1. Identify the component to test
2. Create or update test file in appropriate directory
3. Mock dependencies using Moq
4. Write test following naming convention
5. Use FluentAssertions for assertions
6. Run tests locally before committing

### Updating Existing Tests

- Update test data when domain models change
- Add new test cases for new functionality
- Remove obsolete tests
- Maintain test coverage

## Test Dependencies

### NuGet Packages

| Package | Version | Purpose |
|---------|---------|---------|
| NUnit | 3.x | Test framework |
| Moq | 4.x | Mocking |
| FluentAssertions | 6.x | Assertions |
| Microsoft.NET.Test.Sdk | 17.x | Test runner |
| coverlet.collector | 3.x | Code coverage |

### Test Project Configuration

**File**: `ExonymsAPI.UnitTests/ExonymsAPI.UnitTests.csproj`

```xml
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <TargetFramework>net10.0</TargetFramework>
    <IsPackable>false</IsPackable>
  </PropertyGroup>
  <ItemGroup>
    <PackageReference Include="Microsoft.NET.Test.Sdk" Version="17.*" />
    <PackageReference Include="NUnit" Version="3.*" />
    <PackageReference Include="Moq" Version="4.*" />
    <PackageReference Include="FluentAssertions" Version="6.*" />
    <PackageReference Include="coverlet.collector" Version="3.*" />
  </ItemGroup>
  <ItemGroup>
    <ProjectReference Include="..\ExonymsAPI\ExonymsAPI.csproj" />
  </ItemGroup>
</Project>
```

## Testing Best Practices

### Do

- Test one behaviour per test
- Use descriptive test names
- Mock external dependencies
- Test edge cases and error scenarios
- Keep tests fast and isolated

### Don't

- Don't test implementation details
- Don't use hardcoded external API responses
- Don't share state between tests
- Don't test framework behaviour (NuciAPI, ASP.NET Core)

## Related Documentation

- [Architecture](../architecture.md)
- [Components: Application Services](../components/application-services.md)
- [Error Handling](../error-handling.md)
- [Build and Deployment](../build-and-deployment.md)
- [Change Guide](../change-guide.md)