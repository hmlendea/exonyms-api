# Startup and Rendering Flow

## Application Startup Sequence

```
dotnet run
    │
    ▼
Program.Main(args)
    │
    ▼
CreateHostBuilder(args)
    │
    ├── Host.CreateDefaultBuilder(args)
    │   ├── ConfigureAppConfiguration
    │   │   ├── appsettings.json
    │   │   ├── appsettings.{Environment}.json
    │   │   ├── Environment variables
    │   │   └── Command line args
    │   ├── ConfigureLogging
    │   │   ├── Console
    │   │   ├── Debug
    │   │   └── EventSource
    │   └── ConfigureServices (DI container)
    │
    ▼
webBuilder.UseStartup<Startup>()
    │
    ▼
Startup.ConfigureServices(IServiceCollection)
    │
    ├── services.AddControllers()
    │
    ├── AddConfigurations(Configuration)
    │   ├── Bind TransliterationSettings
    │   ├── Bind SecuritySettings
    │   ├── Bind NuciLoggerSettings
    │   └── Register as Singletons
    │
    └── AddCustomServices()
        ├── Singleton: IExonymsService → ExonymsService
        ├── Singleton: INameNormaliser → NameNormaliser
        ├── Singleton: INameConstructor → NameConstructor
        ├── Singleton: IGeoNamesGatherer → GeoNamesGatherer
        ├── Singleton: IWikiDataGatherer → WikiDataGatherer
        ├── Transient: ITransliterationApiClient → TransliterationApiClient
        ├── Transient: INuciApiClient → NuciApiClient (with TransliterationApiBaseUrl)
        └── Transient: ILogger → NuciLogger
    │
    ▼
Build() → IHost
    │
    ▼
Run() → Start listening
```

## Request Processing Pipeline

```
HTTP Request (GET /Exonyms?geoNamesId=...&wikiDataId=...)
    │
    ▼
Kestrel (HTTP server)
    │
    ▼
Middleware Pipeline (Startup.Configure)
    │
    ├── UseNuciApiRequestLogging
    │   ├── Log request: method, path, query, headers, body
    │   ├── Generate/extract correlation ID
    │   └── Call next()
    │
    ├── UseNuciApiExceptionHandling
    │   ├── try { await next() }
    │   └── catch (Exception ex) → Log, return NuciApiErrorResponse
    │
    ├── UseDeveloperExceptionPage (Development only)
    │
    ├── UseHttpsRedirection
    │
    ├── UseDefaultFiles
    │
    ├── UseStaticFiles
    │
    ├── UseRouting
    │
    ├── UseAuthorization (no-op)
    │
    └── UseEndpoints → MapControllers
        │
        ▼
Controller Action Selection
    │
    ▼
ExonymsController.Get(GetExonymsRequest)
    │
    ├── Model binding: query → GetExonymsRequest { GeoNamesId, WikiDataId }
    │
    ▼
NuciApiController.ProcessRequest(request, handler, NuciApiAuthorisation.None)
    │
    ├── Validate request (if any validation attributes)
    │
    ├── Execute handler delegate:
    │   │
    │   ▼
    │   exonymsService.Gather(geoNamesId, wikiDataId).Result
    │   │
    │   ├── ExonymsService.Gather() → Location (detailed in exonym-gathering.md)
    │   │
    │   ▼
    │   Map Location → GetExonymsResponse
    │       ├── DefaultName = location.DefaultName
    │       ├── Names = location.Names
    │       └── Count = Names.Count (computed)
    │   │
    │   ▼
    │   response.SignHMAC(securitySettings.HmacSigningKey)
    │       └── NuciSecurity.HMAC adds Hmac property
    │   │
    │   ▼
    │   return response
    │
    ├── Wrap in ActionResult (OkObjectResult)
    │
    ▼
Middleware Pipeline (return path)
    │
    ├── UseNuciApiRequestLogging
    │   └── Log response: status, headers, body, duration
    │
    ▼
Kestrel → HTTP Response
```

## Request/Response Logging (NuciApiRequestLogging)

**Request Log Entry**:
```
Operation: HttpRequest
Status: Started
LogInfo:
  - RequestId: <guid>
  - Method: GET
  - Path: /Exonyms
  - Query: geoNamesId=310350&wikiDataId=Q310350
  - Headers: { ... }
  - Body: (empty for GET)
```

**Response Log Entry**:
```
Operation: HttpRequest
Status: Success
LogInfo:
  - RequestId: <guid>
  - StatusCode: 200
  - DurationMs: 1234
  - ResponseBody: { "success": true, "defaultName": "...", ... }
```

## Exception Handling (NuciApiExceptionHandling)

**Unhandled Exception Flow**:
```
Exception thrown in controller/service
    │
    ▼
NuciApiExceptionHandling middleware catches
    │
    ├── Log: Operation=HttpRequest, Status=Failure, Exception=ex
    │
    ├── Create NuciApiErrorResponse
    │   ├── Success = false
    │   ├── Error = { Code, Message }
    │   └── RequestId = correlation ID
    │
    ├── Set status code (500 for unhandled, 400 for validation, etc.)
    │
    ▼
Return JSON error response
```

**Error Response Format**:
```json
{
  "success": false,
  "error": {
    "code": "InternalServerError",
    "message": "Failed to retrieve the WikiData entry for 'Q123': NotFound"
  },
  "requestId": "guid"
}
```

## Controller Action Detail

### ExonymsController.Get

```csharp
[HttpGet]
public ActionResult Get([FromQuery] GetExonymsRequest request)
    => ProcessRequest(
        request,
        () =>
        {
            // 1. Call service (blocking .Result on async)
            Location exonyms = exonymsService.Gather(request.GeoNamesId, request.WikiDataId).Result;

            // 2. Map to response DTO
            GetExonymsResponse response = new()
            {
                DefaultName = exonyms.DefaultName,
                Names = exonyms.Names
            };

            // 3. Sign with HMAC
            response.SignHMAC(securitySettings.HmacSigningKey);

            return response;
        },
        NuciApiAuthorisation.None);
```

### ProcessRequest (NuciApiController)

```csharp
// Simplified signature
protected ActionResult ProcessRequest<TRequest, TResponse>(
    TRequest request,
    Func<TResponse> handler,
    NuciApiAuthorisation authorisation)
{
    // 1. Authorisation check (None = skip)
    // 2. Model validation (if any)
    // 3. Execute handler in try/catch
    // 4. Return Ok(handler()) or error
}
```

## HMAC Signing

**Method**: `NuciApiSuccessResponse.SignHMAC(string key)` (from `NuciSecurity.HMAC`)

**Algorithm**: HMAC-SHA256

**Input**: Serialised response body (excluding Hmac property)

**Output**: `Hmac` property set to `sha256=<base64>`

**Verification** (client side):
1. Remove `hmac` field from response
2. Serialise remaining JSON canonically
3. Compute HMAC-SHA256 with shared key
4. Compare with received `hmac` value

## Static Files & Default Files

- `UseDefaultFiles` + `UseStaticFiles` serves `wwwroot/index.html` at `/`
- Not used by API (no SPA frontend)
- Present for potential future dashboard

## HTTPS Redirection

- `UseHttpsRedirection` redirects HTTP → HTTPS
- Default ports: 5000 (HTTP), 5001 (HTTPS)
- Configurable via `ASPNETCORE_URLS`

## Routing

- Attribute routing only: `[Route("[controller]")]` on `ExonymsController`
- No conventional routing
- Single endpoint: `GET /Exonyms`

## Authorization

- `UseAuthorization` middleware present but no policies configured
- `NuciApiAuthorisation.None` on endpoint
- Effectively public/unauthenticated

## Shutdown Sequence

```
SIGTERM / Ctrl+C
    │
    ▼
IHostApplicationLifetime.StopApplication()
    │
    ▼
Stop accepting new requests
    │
    ▼
Wait for in-flight requests (default timeout: 30s)
    │
    ▼
Dispose DI container (singletons disposed)
    │
    ├── NuciLogger (flush logs)
    │
    ▼
Process exit
```

## Configuration Loading Detail

```
Host.CreateDefaultBuilder
    │
    ▼
ConfigurationBuilder
    │
    ├── AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
    │
    ├── AddJsonFile($"appsettings.{Environment}.json", optional: true, reloadOnChange: true)
    │       └── Environment = ASPNETCORE_ENVIRONMENT (Development/Production)
    │
    ├── AddEnvironmentVariables(prefix: "EXONYMSAPI_")  // Custom prefix if configured
    │       └── Standard: ASPNETCORE_, DOTNET_, etc.
    │
    ├── AddCommandLine(args)
    │
    └── AddUserSecrets (Development only)
    │
    ▼
IConfiguration (merged, precedence: last wins)
    │
    ▼
Startup.ConfigureServices
    │
    ▼
configuration.Bind("TransliterationSettings", transliterationSettings)
    │
    ▼
services.AddSingleton(transliterationSettings)
```

## Logging Initialisation

```
NuciLoggerSettings from configuration
    │
    ▼
NuciLog package internal setup
    │
    ├── Console sink (always)
    ├── File sink (if IsFileOutputEnabled, path: LogFilePath)
    │
    ▼
ILogger registered as Transient (NuciLogger)
    │
    ▼
Injected into services → structured logging with MyOperation/MyLogInfoKey
```