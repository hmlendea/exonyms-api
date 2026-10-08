# Change Guide

## Overview

This guide provides procedures for making changes to the Exonyms API codebase, including development workflow, testing requirements, and deployment processes.

## Development Workflow

### Branch Strategy

```
main (protected)
  │
  ├── develop (integration branch)
  │     │
  │     ├── feature/xxx (feature branches)
  │     ├── bugfix/xxx (bug fix branches)
  │     └── hotfix/xxx (urgent fixes)
  │
  └── release/x.x.x (release branches)
```

### Creating a Change

1. **Create branch** from `develop`:
   ```bash
   git checkout develop
   git pull origin develop
   git checkout -b feature/your-feature-name
   ```

2. **Make changes** following coding standards

3. **Run tests locally**:
   ```bash
   dotnet test ExonymsAPI.sln --configuration Release
   ```

4. **Commit changes** with conventional commits:
   ```bash
   git add .
   git commit -m "feat: add new transliteration language support"
   ```

5. **Push and create PR**:
   ```bash
   git push origin feature/your-feature-name
   # Create PR via GitHub UI
   ```

### Pull Request Requirements

- [ ] All tests pass
- [ ] Code follows style guidelines
- [ ] Documentation updated
- [ ] No breaking changes (or documented)
- [ ] Reviewed by at least one maintainer

## Coding Standards

### C# Style

- Follow [Microsoft C# Coding Conventions](https://docs.microsoft.com/en-us/dotnet/csharp/fundamentals/coding-style/coding-conventions)
- Use `var` when type is obvious
- Prefer expression-bodied members
- Use nullable reference types

### Naming Conventions

| Element | Convention | Example |
|---------|------------|---------|
| Classes | PascalCase | `ExonymsService` |
| Interfaces | IPascalCase | `IExonymsService` |
| Methods | PascalCase | `GatherAsync` |
| Properties | PascalCase | `DefaultName` |
| Fields (private) | _camelCase | `_logger` |
| Parameters | camelCase | `geoNamesId` |
| Constants | PascalCase | `DefaultLanguageCode` |

### Async/Await

- Use `async`/`await` consistently
- Avoid `.Result` and `.Wait()`
- Name async methods with `Async` suffix

### Error Handling

- Use exceptions for exceptional cases
- Don't catch and rethrow without adding context
- Log errors with correlation IDs

## Testing Requirements

### Unit Tests

**Required for**:
- New public methods
- Bug fixes
- Logic changes

**Location**: `ExonymsAPI.UnitTests/`

**Naming**: `MethodName_StateUnderTest_ExpectedBehavior`

**Example**:
```csharp
[Test]
public void Gather_WithBothIds_WikiDataTakesPrecedence()
{
    // Arrange
    // Act
    // Assert
}
```

### Integration Tests

**Required for**:
- New external API integrations
- Middleware changes
- Configuration changes

**Location**: `ExonymsAPI.IntegrationTests/` (to be created)

### Test Coverage

**Target**: > 80% line coverage

**Check**:
```bash
dotnet test --collect:"XPlat Code Coverage"
```

## Documentation Updates

### When to Update Documentation

| Change Type | Documentation to Update |
|-------------|------------------------|
| New endpoint | API reference, behaviour |
| New configuration | Configuration guide |
| New external API | Integrations, components |
| Logic change | Flows, components, invariants |
| Breaking change | Change guide, API reference |
| New dependency | Dependencies |

### Documentation Files

| File | Purpose |
|------|---------|
| `docs/architecture.md` | High-level architecture |
| `docs/components/` | Component documentation |
| `docs/flows/` | Data flow documentation |
| `docs/behaviour/` | User-facing behaviour |
| `docs/api-reference/` | API endpoint documentation |
| `docs/configuration.md` | Configuration reference |
| `docs/dependencies.md` | External dependencies |
| `docs/design-decisions.md` | Design rationale |
| `docs/testing.md` | Testing strategy |
| `docs/security.md` | Security considerations |

## Configuration Changes

### Adding New Settings

1. **Create settings class**:
   ```csharp
   public class NewFeatureSettings
   {
       public string ApiUrl { get; set; }
       public int TimeoutSeconds { get; set; } = 30;
   }
   ```

2. **Add to appsettings.json**:
   ```json
   {
     "NewFeatureSettings": {
       "ApiUrl": "https://api.example.com",
       "TimeoutSeconds": 30
     }
   }
   ```

3. **Register in ServiceCollectionExtensions**:
   ```csharp
   services.Configure<NewFeatureSettings>(configuration.GetSection("NewFeatureSettings"));
   services.AddSingleton(resolver => resolver.GetRequiredService<IOptions<NewFeatureSettings>>().Value);
   ```

4. **Inject where needed**:
   ```csharp
   public MyService(NewFeatureSettings settings) { ... }
   ```

### Environment-Specific Settings

- `appsettings.json` - Base settings
- `appsettings.Development.json` - Development overrides
- `appsettings.Production.json` - Production overrides
- Environment variables - Highest precedence

## External API Changes

### Adding New External API

1. **Create client interface**:
   ```csharp
   public interface INewApiClient
   {
       Task<Response> GetDataAsync(Request request);
   }
   ```

2. **Implement client**:
   ```csharp
   public class NewApiClient : INewApiClient
   {
       private readonly NuciApiClient _apiClient;

       public NewApiClient(NuciApiClient apiClient) { ... }

       public async Task<Response> GetDataAsync(Request request) { ... }
   }
   ```

3. **Add settings**:
   ```csharp
   public class NewApiSettings
   {
       public string BaseUrl { get; set; }
   }
   ```

4. **Register in DI**:
   ```csharp
   services.AddTransient<INewApiClient, NewApiClient>();
   services.AddTransient<NuciApiClient>(sp => new NewApiClient(sp.GetRequiredService<NewApiSettings>().BaseUrl));
   ```

5. **Add tests**:
   - Unit tests with mocked client
   - Integration tests (optional)

### Updating Existing External API

1. **Check breaking changes** in external API
2. **Update client** to handle new response format
3. **Update tests** with new mock data
4. **Update documentation** in `integrations.md`

## Database Changes

**Not applicable** - No database in this project.

## Breaking Changes

### Definition

A breaking change is any change that requires clients to modify their code:

- Removing/renaming API fields
- Changing response format
- Changing HTTP status codes
- Removing endpoints
- Changing parameter requirements

### Process

1. **Document** in PR description
2. **Update version** to next major (SemVer)
3. **Update API reference** documentation
4. **Communicate** to consumers
5. **Provide migration guide**

### Versioning

| Change Type | Version Bump |
|-------------|--------------|
| Bug fix | PATCH (1.0.1) |
| New feature (backward compatible) | MINOR (1.1.0) |
| Breaking change | MAJOR (2.0.0) |

## Release Process

### Pre-Release Checklist

- [ ] All tests pass
- [ ] Documentation updated
- [ ] Version bumped in `ExonymsAPI.csproj`
- [ ] CHANGELOG.md updated (if exists)
- [ ] Security scan passed
- [ ] Performance benchmarks run

### Release Steps

1. **Create release branch**:
   ```bash
   git checkout develop
   git pull origin develop
   git checkout -b release/1.2.0
   ```

2. **Update version**:
   ```xml
   <!-- ExonymsAPI.csproj -->
   <Version>1.2.0</Version>
   ```

3. **Commit and tag**:
   ```bash
   git commit -am "chore: release v1.2.0"
   git tag -a v1.2.0 -m "Release v1.2.0"
   ```

4. **Merge to main**:
   ```bash
   git checkout main
   git merge release/1.2.0
   git push origin main --tags
   ```

5. **Merge back to develop**:
   ```bash
   git checkout develop
   git merge release/1.2.0
   git push origin develop
   ```

6. **Clean up**:
   ```bash
   git branch -d release/1.2.0
   ```

### Post-Release

- [ ] Verify GitHub Actions deployment
- [ ] Verify NuGet package (if applicable)
- [ ] Monitor for issues
- [ ] Update deployment documentation

## Hotfix Process

### When to Hotfix

- Critical production bug
- Security vulnerability
- Data corruption issue

### Hotfix Steps

1. **Create hotfix branch** from `main`:
   ```bash
   git checkout main
   git pull origin main
   git checkout -b hotfix/1.2.1
   ```

2. **Fix issue** with minimal change

3. **Test thoroughly**

4. **Update version** (PATCH):
   ```xml
   <Version>1.2.1</Version>
   ```

5. **Release** (same as release process)

6. **Merge to develop**:
   ```bash
   git checkout develop
   git merge hotfix/1.2.1
   git push origin develop
   ```

## Rollback Process

### Code Rollback

```bash
# Revert commit
git revert <commit-hash>
git push origin main

# Or reset (if not pushed)
git reset --hard <commit-hash>
git push origin main --force
```

### Deployment Rollback

See [Build and Deployment](../build-and-deployment.md#rollback-procedure)

## Dependency Updates

### Regular Updates

```bash
# Check for updates
dotnet list package --outdated

# Update specific package
dotnet add ExonymsAPI/ExonymsAPI.csproj package PackageName --version x.y.z

# Update all (use with caution)
dotnet add ExonymsAPI/ExonymsAPI.csproj package PackageName
```

### Security Updates

```bash
# Check vulnerabilities
dotnet list package --vulnerable

# Update vulnerable packages immediately
dotnet add ExonymsAPI/ExonymsAPI.csproj package VulnerablePackage --version safe-version
```

### Testing After Updates

1. Run full test suite
2. Run integration tests
3. Manual smoke test
4. Deploy to staging
5. Monitor for issues

## Code Review Guidelines

### For Authors

- Keep PRs small (< 400 lines)
- Write clear commit messages
- Include tests
- Update documentation
- Self-review before requesting review

### For Reviewers

- Check for correctness
- Verify tests cover changes
- Check for security issues
- Verify documentation updates
- Check for performance implications
- Approve or request changes

## Emergency Procedures

### Production Incident

1. **Assess** impact and severity
2. **Communicate** to team
3. **Mitigate** (rollback, hotfix, config change)
4. **Monitor** recovery
5. **Post-mortem** within 48 hours

### Security Incident

1. **Contain** (rotate keys, disable endpoints)
2. **Assess** scope
3. **Notify** stakeholders
4. **Remediate**
5. **Document** and improve

## Related Documentation

- [Architecture](../architecture.md)
- [Build and Deployment](../build-and-deployment.md)
- [Testing](../testing.md)
- [Security](../security.md)
- [Documentation Maintenance](../documentation-maintenance.md)