# Build and Deployment

## Overview

The Exonyms API uses standard .NET build and deployment processes with GitHub Actions for CI/CD.

## Build Process

### Prerequisites

| Tool | Version | Purpose |
|------|---------|---------|
| .NET SDK | 10.0 | Build and run |
| Git | Latest | Source control |

### Local Build

```bash
# Restore dependencies
dotnet restore

# Build solution
dotnet build ExonymsAPI.sln --configuration Release

# Run tests
dotnet test ExonymsAPI.sln --configuration Release --no-build

# Publish
dotnet publish ExonymsAPI/ExonymsAPI.csproj --configuration Release --output ./publish
```

### Build Output

```
publish/
├── ExonymsAPI.dll
├── ExonymsAPI.deps.json
├── ExonymsAPI.runtimeconfig.json
├── appsettings.json
├── appsettings.Development.json
├── appsettings.Production.json
└── (dependencies)
```

## CI Pipeline

### GitHub Actions Workflow

**File**: `.github/workflows/dotnet.yml`

```yaml
name: .NET CI

on:
  push:
    branches: [main, develop]
  pull_request:
    branches: [main, develop]

jobs:
  build:
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v4

      - name: Setup .NET
        uses: actions/setup-dotnet@v4
        with:
          dotnet-version: '10.0.x'

      - name: Restore
        run: dotnet restore ExonymsAPI.sln

      - name: Build
        run: dotnet build ExonymsAPI.sln --configuration Release --no-restore

      - name: Test
        run: dotnet test ExonymsAPI.sln --configuration Release --no-build --verbosity normal
```

### CI Stages

| Stage | Command | Purpose |
|-------|---------|---------|
| Checkout | `actions/checkout@v4` | Get source code |
| Setup .NET | `actions/setup-dotnet@v4` | Install .NET SDK |
| Restore | `dotnet restore` | Download NuGet packages |
| Build | `dotnet build` | Compile solution |
| Test | `dotnet test` | Run unit tests |

## Release Process

### Release Script

**File**: `release.sh`

```bash
#!/bin/bash
# Delegates to deployment-scripts repository
./deployment-scripts/release.sh "$@"
```

### Versioning

**Scheme**: Semantic Versioning (SemVer)

**Format**: `MAJOR.MINOR.PATCH`

**Examples**:
- `1.0.0` - Initial release
- `1.1.0` - New feature (backward compatible)
- `1.1.1` - Bug fix
- `2.0.0` - Breaking change

### Release Steps

1. **Update version** in `ExonymsAPI.csproj`:
   ```xml
   <Version>1.2.3</Version>
   ```

2. **Create release commit**:
   ```bash
   git commit -am "chore: release v1.2.3"
   ```

3. **Tag release**:
   ```bash
   git tag -a v1.2.3 -m "Release v1.2.3"
   ```

4. **Push**:
   ```bash
   git push origin main --tags
   ```

5. **GitHub Actions** automatically:
   - Builds and tests
   - Creates GitHub Release
   - Publishes to NuGet (if configured)

## Deployment

### Target Environments

| Environment | URL | Purpose |
|-------------|-----|---------|
| Development | `http://localhost:5000` | Local development |
| Staging | `https://staging.api.example.com` | Pre-production testing |
| Production | `https://api.example.com` | Live traffic |

### Deployment Methods

#### Docker (Recommended)

**Dockerfile** (not currently present, recommended):

```dockerfile
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY ["ExonymsAPI/ExonymsAPI.csproj", "ExonymsAPI/"]
RUN dotnet restore "ExonymsAPI/ExonymsAPI.csproj"
COPY . .
WORKDIR "/src/ExonymsAPI"
RUN dotnet publish -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "ExonymsAPI.dll"]
```

**Build and Run**:
```bash
docker build -t exonyms-api .
docker run -p 8080:8080 exonyms-api
```

#### Direct Deployment

```bash
# On target server
dotnet ExonymsAPI.dll --urls "http://0.0.0.0:5000"
```

#### Systemd Service (Linux)

**File**: `/etc/systemd/system/exonyms-api.service`

```ini
[Unit]
Description=Exonyms API
After=network.target

[Service]
Type=notify
ExecStart=/usr/bin/dotnet /opt/exonyms-api/ExonymsAPI.dll
WorkingDirectory=/opt/exonyms-api
Environment=ASPNETCORE_ENVIRONMENT=Production
Environment=ASPNETCORE_URLS=http://0.0.0.0:5000
Restart=always
RestartSec=10
User=exonyms-api
Group=exonyms-api

[Install]
WantedBy=multi-user.target
```

**Enable and Start**:
```bash
sudo systemctl enable exonyms-api
sudo systemctl start exonyms-api
```

### Configuration per Environment

#### Development

```json
{
  "NuciLoggerSettings": {
    "IsFileOutputEnabled": false,
    "MinimumLevel": "Debug"
  }
}
```

#### Staging

```json
{
  "NuciLoggerSettings": {
    "IsFileOutputEnabled": true,
    "LogFilePath": "/var/log/exonyms-api/staging.log",
    "MinimumLevel": "Information"
  },
  "TransliterationSettings": {
    "TransliterationApiBaseUrl": "https://staging-translit.example.com"
  }
}
```

#### Production

```json
{
  "NuciLoggerSettings": {
    "IsFileOutputEnabled": true,
    "LogFilePath": "/var/log/exonyms-api/production.log",
    "MinimumLevel": "Information"
  },
  "TransliterationSettings": {
    "TransliterationApiBaseUrl": "https://translit.example.com"
  },
  "SecuritySettings": {
    "HmacSigningKey": "<from-secrets>"
  }
}
```

## Environment Variables

| Variable | Description | Required |
|----------|-------------|----------|
| `ASPNETCORE_ENVIRONMENT` | Environment name (Development/Staging/Production) | Yes |
| `ASPNETCORE_URLS` | URLs to listen on | No (default: http://localhost:5000) |
| `TransliterationSettings__TransliterationApiBaseUrl` | Transliteration API URL | Yes |
| `SecuritySettings__HmacSigningKey` | HMAC signing key | Yes |

## Health Checks

### Current State

**Not implemented**. No health check endpoints.

### Recommended Implementation

```csharp
// In Startup.cs
services.AddHealthChecks()
    .AddCheck("self", () => HealthCheckResult.Healthy())
    .AddUrlGroup(new Uri("https://wikidata.org"), "wikidata")
    .AddUrlGroup(new Uri("http://api.geonames.org"), "geonames");

// In Program.cs
app.MapHealthChecks("/health");
```

### Health Check Endpoints

| Endpoint | Purpose |
|----------|---------|
| `GET /health` | Overall health |
| `GET /health/ready` | Readiness probe |
| `GET /health/live` | Liveness probe |

## Monitoring

### Metrics

**Not implemented**. Recommended:

```csharp
// Add Prometheus metrics
services.AddMetrics()
    .AddPrometheusExporter();
```

### Logging

See [Logging](../logging.md) for details.

### Tracing

**Not implemented**. Recommended: OpenTelemetry.

## Rollback Procedure

### GitHub Release Rollback

1. **Delete release** on GitHub
2. **Delete tag** locally and remotely:
   ```bash
   git tag -d v1.2.3
   git push origin :refs/tags/v1.2.3
   ```
3. **Revert commit** if needed:
   ```bash
   git revert <commit-hash>
   git push origin main
   ```

### Deployment Rollback

```bash
# Docker
docker tag exonyms-api:previous exonyms-api:latest
docker service update --image exonyms-api:latest exonyms-api

# Systemd
sudo systemctl stop exonyms-api
# Restore previous binary
sudo systemctl start exonyms-api
```

## Security Considerations

### Build Security

- Use pinned dependency versions
- Scan for vulnerabilities: `dotnet list package --vulnerable`
- Sign assemblies (if required)

### Deployment Security

- Use secrets management (Azure Key Vault, AWS Secrets Manager, HashiCorp Vault)
- Never commit secrets to repository
- Rotate HMAC keys periodically
- Use HTTPS in production

## Performance Optimization

### Build Optimizations

```xml
<!-- In ExonymsAPI.csproj -->
<PropertyGroup>
  <PublishTrimmed>true</PublishTrimmed>
  <PublishSingleFile>true</PublishSingleFile>
  <PublishReadyToRun>true</PublishReadyToRun>
</PropertyGroup>
```

### Runtime Optimizations

- Enable tiered compilation (default in .NET 10)
- Use `Server GC` for throughput
- Configure thread pool for expected load

## Troubleshooting

### Common Build Issues

| Issue | Solution |
|-------|----------|
| NuGet restore fails | Check network, clear cache: `dotnet nuget locals all --clear` |
| Build fails on CI | Check .NET version matches local |
| Tests fail | Run locally with same configuration |

### Common Deployment Issues

| Issue | Solution |
|-------|----------|
| Port already in use | Change `ASPNETCORE_URLS` |
| Configuration missing | Verify environment variables |
| External API timeout | Check network/firewall |
| HMAC verification fails | Verify key matches |

## Related Documentation

- [Architecture](../architecture.md)
- [Configuration](../configuration.md)
- [Security](../security.md)
- [Testing](../testing.md)
- [Change Guide](../change-guide.md)