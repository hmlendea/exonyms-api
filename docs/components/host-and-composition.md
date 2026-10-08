# Host and Composition

## Program.cs

**File**: `ExonymsAPI/Program.cs`
**Namespace**: `ExonymsAPI`

```csharp
public class Program
{
    public static void Main(string[] args)
    {
        CreateHostBuilder(args).Build().Run();
    }

    public static IHostBuilder CreateHostBuilder(string[] args) =>
        Host.CreateDefaultBuilder(args)
            .ConfigureWebHostDefaults(webBuilder =>
            {
                webBuilder.UseStartup<Startup>();
            });
}
```

### Responsibilities

- Entry point (`Main`)
- Generic host builder configuration
- Delegates to `Startup` for application configuration
- Uses `Host.CreateDefaultBuilder` (standard ASP.NET Core defaults):
  - Configuration: `appsettings.json`, `appsettings.{Environment}.json`, env vars, command line
  - Logging: Console, Debug, EventSource
  - Dependency injection container
  - Host lifetime management

## Startup.cs

**File**: `ExonymsAPI/Startup.cs`
**Namespace**: `ExonymsAPI`

```csharp
public class Startup(IConfiguration configuration)
{
    public IConfiguration Configuration { get; } = configuration;

    public void ConfigureServices(IServiceCollection services)
    {
        services.AddControllers();

        services
            .AddConfigurations(Configuration)
            .AddCustomServices();
    }

    public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
    {
        app.UseNuciApiRequestLogging();
        app.UseNuciApiExceptionHandling();

        if (env.IsDevelopment())
        {
            app.UseDeveloperExceptionPage();
        }

        app.UseHttpsRedirection();
        app.UseDefaultFiles();
        app.UseStaticFiles();
        app.UseRouting();
        app.UseAuthorization();

        app.UseEndpoints(endpoints =>
        {
            endpoints.MapControllers();
        });
    }
}
```

### ConfigureServices

1. **`services.AddControllers()`**: Adds MVC controller support (routing, model binding, validation)
2. **`AddConfigurations(Configuration)`**: Binds and registers settings objects
3. **`AddCustomServices()`**: Registers application services (gatherers, processors, clients)

### Configure (Middleware Pipeline)

| Order | Middleware | Purpose |
|-------|------------|---------|
| 1 | `UseNuciApiRequestLogging` | Request/response logging (NuciAPI) |
| 2 | `UseNuciApiExceptionHandling` | Global exception handling (NuciAPI) |
| 3 | `UseDeveloperExceptionPage` | Detailed errors in Development |
| 4 | `UseHttpsRedirection` | HTTP → HTTPS redirect |
| 5 | `UseDefaultFiles` | Serve default files (index.html) |
| 6 | `UseStaticFiles` | Serve static files (wwwroot) |
| 7 | `UseRouting` | Endpoint routing |
| 8 | `UseAuthorization` | Authorization (no-op, no policies) |
| 9 | `UseEndpoints` | Map controllers |

## ServiceCollectionExtensions.cs

**File**: `ExonymsAPI/ServiceCollectionExtensions.cs`
**Namespace**: `ExonymsAPI`

### AddConfigurations

```csharp
public static IServiceCollection AddConfigurations(
    this IServiceCollection services,
    IConfiguration configuration)
{
    TransliterationSettings transliterationSettings = new();
    SecuritySettings securitySettings = new();
    NuciLoggerSettings nuciLoggerSettings = new();

    configuration.Bind(nameof(TransliterationSettings), transliterationSettings);
    configuration.Bind(nameof(SecuritySettings), securitySettings);
    configuration.Bind(nameof(NuciLoggerSettings), nuciLoggerSettings);

    services.AddSingleton(transliterationSettings);
    services.AddSingleton(securitySettings);
    services.AddSingleton(nuciLoggerSettings);

    return services;
}
```

- Binds configuration sections to POCO settings classes
- Registers as singletons for DI injection
- Section names match class names exactly

### AddCustomServices

```csharp
public static IServiceCollection AddCustomServices(
    this IServiceCollection services) => services
    .AddSingleton<IExonymsService, ExonymsService>()
    .AddSingleton<INameNormaliser, NameNormaliser>()
    .AddSingleton<INameConstructor, NameConstructor>()
    .AddSingleton<IGeoNamesGatherer, GeoNamesGatherer>()
    .AddSingleton<IWikiDataGatherer, WikiDataGatherer>()
    .AddTransient<ITransliterationApiClient, TransliterationApiClient>()
    .AddTransient<INuciApiClient>(provider =>
        new NuciApiClient(provider.GetRequiredService<TransliterationSettings>().TransliterationApiBaseUrl))
    .AddTransient<ILogger, NuciLogger>();
```

### Lifetime Decisions

| Service | Lifetime | Rationale |
|---------|----------|-----------|
| `IExonymsService` | Singleton | Stateless orchestrator |
| `INameNormaliser` | Singleton | Pure functions, no state |
| `INameConstructor` | Singleton | Readonly transformation dictionary |
| `IGeoNamesGatherer` | Singleton | Stateless, usernames `readonly` |
| `IWikiDataGatherer` | Singleton | Stateless |
| `ITransliterationApiClient` | Transient | Wraps `NuciApiClient` (transient) |
| `INuciApiClient` | Transient | `HttpClient` per instance (via factory) |
| `ILogger` | Transient | `NuciLogger` per scope |

### NuciApiClient Registration

```csharp
.AddTransient<INuciApiClient>(provider =>
    new NuciApiClient(provider.GetRequiredService<TransliterationSettings>().TransliterationApiBaseUrl))
```

- Factory delegate resolves `TransliterationSettings` at creation time
- Base URL injected into `NuciApiClient` constructor
- Transient ensures fresh `HttpClient` per request (via `NuciApiClient` internals)

## Dependency Injection Graph

```
IServiceCollection
├── Controllers (AddControllers)
│   └── ExonymsController
│       ├── IExonymsService (Singleton) → ExonymsService
│       │   ├── IGeoNamesGatherer (Singleton) → GeoNamesGatherer
│       │   │   ├── INameNormaliser (Singleton) → NameNormaliser
│       │   │   └── ITransliterationApiClient (Transient) → TransliterationApiClient
│       │   │       └── INuciApiClient (Transient) → NuciApiClient
│       │   ├── IWikiDataGatherer (Singleton) → WikiDataGatherer
│       │   │   ├── INameNormaliser (Singleton) → NameNormaliser
│       │   │   └── ITransliterationApiClient (Transient) → TransliterationApiClient
│       │   ├── INameConstructor (Singleton) → NameConstructor
│       │   ├── INameNormaliser (Singleton) → NameNormaliser
│       │   └── ITransliterationApiClient (Transient) → TransliterationApiClient
│       └── SecuritySettings (Singleton)
├── Configuration (Singletons)
│   ├── TransliterationSettings
│   ├── SecuritySettings
│   └── NuciLoggerSettings
└── ILogger (Transient) → NuciLogger
```

## Configuration Flow

```
appsettings.json / Environment Variables
    ↓
IConfiguration (in Startup constructor)
    ↓
ServiceCollectionExtensions.AddConfigurations()
    ↓
configuration.Bind(nameof(Settings), settingsInstance)
    ↓
services.AddSingleton(settingsInstance)
    ↓
Injected into:
    - TransliterationApiClient (via NuciApiClient factory)
    - ExonymsController (SecuritySettings)
    - NuciLogger (NuciLoggerSettings, internal)
```

## Hosting Model

- **Kestrel**: Default ASP.NET Core web server
- **Ports**: HTTP (5000), HTTPS (5001) by default (configurable via `ASPNETCORE_URLS`)
- **Content Root**: Project directory
- **Web Root**: `wwwroot` (for static files)
- **Environment**: `ASPNETCORE_ENVIRONMENT` (Development/Production)

## Startup Sequence

1. `Program.Main()` → `CreateHostBuilder()` → `Build()` → `Run()`
2. Host builds configuration, logging, DI container
3. `Startup.ConfigureServices()` → registers all services
4. `Startup.Configure()` → builds middleware pipeline
5. Application listens for requests

## Shutdown

- `IHostApplicationLifetime` signals (Ctrl+C, SIGTERM)
- Graceful shutdown: stops accepting requests, completes in-flight
- No explicit cleanup (no persistent connections, no background services)

## Extensibility Points

### Adding a New Gatherer

1. Create `INewGatherer` interface and `NewGatherer` implementation
2. Register in `AddCustomServices()`: `.AddSingleton<INewGatherer, NewGatherer>()`
3. Inject into `ExonymsService` constructor
4. Call in `Gather()` method

### Adding a New Processor

1. Create `INewProcessor` interface and implementation
2. Register as Singleton in `AddCustomServices()`
3. Inject into `ExonymsService` or gatherers as needed

### Adding Middleware

1. Add `app.UseNewMiddleware()` in `Startup.Configure()` at appropriate position
2. Register any required services in `AddCustomServices()`

### Adding Configuration

1. Create `NewSettings` class in `Configuration/`
2. Add section to `appsettings.json`
3. Bind and register in `AddConfigurations()`
4. Inject where needed