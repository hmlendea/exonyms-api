# Security

## Overview

The Exonyms API implements security measures for response integrity, configuration management, and external API access. This document covers all security considerations and best practices.

## Security Architecture

```
┌─────────────────────────────────────────────────────────────┐
│                    Security Layers                             │
├─────────────────────────────────────────────────────────────┤
│                                                               │
│  Layer 1: Response Integrity (HMAC-SHA256)                    │
│  ┌─────────────────────────────────────────────────────┐     │
│  │ NuciSecurity.HMAC                                     │     │
│  │ - All responses signed                                │     │
│  │ - Clients verify integrity                            │     │
│  └─────────────────────────────────────────────────────┘     │
│                                                               │
│  Layer 2: Configuration Security                              │
│  ┌─────────────────────────────────────────────────────┐     │
│  │ SecuritySettings.HmacSigningKey                       │     │
│  │ - Loaded from configuration                           │     │
│  │ - Never logged or exposed                             │     │
│  └─────────────────────────────────────────────────────┘     │
│                                                               │
│  Layer 3: External API Access                                 │
│  ┌─────────────────────────────────────────────────────┐     │
│  │ GeoNames usernames (hardcoded)                        │     │
│  │ WikiData (no auth)                                    │     │
│  │ Transliteration API (configurable)                    │     │
│  └─────────────────────────────────────────────────────┘     │
│                                                               │
└─────────────────────────────────────────────────────────────┘
```

## Response Integrity

### HMAC-SHA256 Signing

**Implementation**: `NuciSecurity.HMAC` package

**Algorithm**: HMAC-SHA256

**Key**: `SecuritySettings.HmacSigningKey`

**Format**: `sha256=<base64>`

### Signing Process

```csharp
// In ExonymsController
response.SignHMAC(securitySettings.HmacSigningKey);
```

**Steps**:
1. Serialise response (excluding `hmac` field)
2. Compute HMAC-SHA256 with signing key
3. Add `sha256=` prefix
4. Set `Hmac` property

### Verification (Client Side)

```csharp
public bool VerifyHmac(GetExonymsResponse response, string key)
{
    string receivedHmac = response.Hmac;
    response.Hmac = null;

    string json = JsonSerializer.Serialize(response, options);
    string computedHmac = ComputeHmacSha256(json, key);

    return receivedHmac == $"sha256={computedHmac}";
}
```

### Security Benefits

- **Integrity**: Detects tampering in transit
- **Authenticity**: Confirms response came from the API
- **Non-repudiation**: Server cannot deny sending response

## Configuration Security

### HMAC Signing Key

**Location**: `SecuritySettings.HmacSigningKey`

**Configuration**:
```json
{
  "SecuritySettings": {
    "HmacSigningKey": "<your-secret-key>"
  }
}
```

**Security Requirements**:
- Minimum 32 characters
- Randomly generated
- Never committed to repository
- Rotated periodically

**Generation**:
```bash
# Generate secure key
dotnet tool install --global System.Security.Cryptography.OpenSsl
openssl rand -base64 32
```

### Transliteration API URL

**Location**: `TransliterationSettings.TransliterationApiBaseUrl`

**Configuration**:
```json
{
  "TransliterationSettings": {
    "TransliterationApiBaseUrl": "https://api.example.com/translit"
  }
}
```

**Security**:
- Use HTTPS
- Validate certificate
- Consider API key authentication

## Hardcoded Credentials

### GeoNames Usernames

**Location**: `GeoNamesGatherer.usernames`

**Current State**: 60+ usernames hardcoded in source code

**Security Risk**: **HIGH** - Credentials exposed in source code

**Mitigation Options**:

1. **Move to configuration** (recommended):
   ```json
   {
     "GeoNamesSettings": {
       "Usernames": ["username1", "username2", ...]
     }
   }
   ```

2. **Use secrets manager**:
   - Azure Key Vault
   - AWS Secrets Manager
   - HashiCorp Vault

3. **Environment variables**:
   ```bash
   GEONAMES_USERNAMES=username1,username2,...
   ```

### Migration Plan

```csharp
// Current
private static string[] usernames = { ... };

// Future
private readonly string[] usernames;

public GeoNamesGatherer(IConfiguration configuration)
{
    usernames = configuration.GetSection("GeoNamesSettings:Usernames")
        .Get<string[]>() ?? Array.Empty<string>();
}
```

## Input Validation

### Request Validation

**Current**: Basic validation in controller

```csharp
if (request.GeoNamesId == null && string.IsNullOrEmpty(request.WikiDataId))
{
    return BadRequest("At least one of geoNamesId or wikiDataId must be provided.");
}
```

**Recommended Enhancements**:

```csharp
// Add validation attributes
public class GetExonymsRequest
{
    [Range(1, int.MaxValue, ErrorMessage = "geoNamesId must be a positive integer")]
    public int? GeoNamesId { get; set; }

    [RegularExpression(@"^Q\d+$", ErrorMessage = "wikiDataId must be in format Q123456")]
    public string WikiDataId { get; set; }
}
```

### SQL Injection Prevention

**Status**: Not applicable - no database access.

### Command Injection Prevention

**Status**: Not applicable - no shell commands.

### Path Traversal Prevention

**Status**: Not applicable - no file system access.

## External API Security

### GeoNames

| Aspect | Status |
|--------|--------|
| Authentication | Username in query string |
| HTTPS | No (HTTP only) |
| Rate limiting | ~2000 req/hour per username |

**Recommendations**:
- Use HTTPS if available
- Rotate usernames
- Implement rate limiting

### WikiData

| Aspect | Status |
|--------|--------|
| Authentication | None |
| HTTPS | Yes |
| Rate limiting | No strict limit |

**Recommendations**:
- Be respectful (implement rate limiting)
- Cache responses
- Use proper User-Agent header

### Transliteration API

| Aspect | Status |
|--------|--------|
| Authentication | Configurable |
| HTTPS | Recommended |
| Rate limiting | Service-dependent |

**Recommendations**:
- Use HTTPS
- Implement API key authentication
- Monitor usage

## HTTPS Configuration

### Current State

**Enabled**: `UseHttpsRedirection` middleware

**Default Ports**: 5000 (HTTP), 5001 (HTTPS)

### Production Configuration

```json
{
  "Kestrel": {
    "Endpoints": {
      "Http": {
        "Url": "http://0.0.0.0:5000"
      },
      "Https": {
        "Url": "https://0.0.0.0:5001",
        "Certificate": {
          "Path": "/path/to/certificate.pfx",
          "Password": "<password>"
        }
      }
    }
  }
}
```

### Docker Configuration

```dockerfile
ENV ASPNETCORE_URLS=https://+:443;http://+:80
ENV ASPNETCORE_Kestrel__Certificates__Default__Path=/certs/certificate.pfx
ENV ASPNETCORE_Kestrel__Certificates__Default__Password=<password>
```

## Secrets Management

### Current State

**Secrets in**:
- `appsettings.json` (development)
- Environment variables (production)

**Not in**:
- Source code (except GeoNames usernames)
- Git history

### Recommended Secrets Management

**Azure Key Vault**:
```csharp
services.AddAzureKeyVault(
    new Uri("https://<vault>.vault.azure.net/"),
    new DefaultAzureCredential());
```

**AWS Secrets Manager**:
```csharp
services.AddAWSSecretsManager(configuration);
```

**HashiCorp Vault**:
```csharp
services.AddVault(configuration);
```

## Logging Security

### Sensitive Data Masking

**Current State**: No explicit masking configured

**Recommended**:
```csharp
// Mask sensitive headers
logger.Mask("Authorization", "Cookie", "X-Api-Key");
```

### Log Content

**Never log**:
- HMAC signing key
- API credentials
- User data (PII)
- Full request/response bodies (in production)

**Log**:
- Request path and query (sanitized)
- Error messages (sanitized)
- Correlation IDs

## Dependency Security

### NuGet Package Auditing

```bash
# Audit packages for vulnerabilities
dotnet list package --vulnerable

# Update vulnerable packages
dotnet add package <package> --version <safe-version>
```

### Current Dependencies

| Package | Version | Known Vulnerabilities |
|---------|---------|----------------------|
| Newtonsoft.Json | 13.0.4 | Check CVE database |
| NuciAPI | 3.3.0 | Check CVE database |
| NuciLog | 1.1.2 | Check CVE database |
| NuciSecurity.HMAC | 4.1.2 | Check CVE database |
| NuciWeb.HTTP | 1.2.0 | Check CVE database |

### Dependency Updates

**Schedule**: Monthly security audits

**Process**:
1. Run `dotnet list package --vulnerable`
2. Review CVEs
3. Update packages
4. Run tests
5. Deploy

## Security Testing

### Unit Tests

```csharp
[Test]
public void SignHMAC_GeneratesValidSignature()
{
    var response = new GetExonymsResponse { DefaultName = "Test" };
    response.SignHMAC("secret-key");

    response.Hmac.Should().StartWith("sha256=");
}

[Test]
public void VerifyHmac_WithCorrectKey_ReturnsTrue()
{
    var response = new GetExonymsResponse { DefaultName = "Test" };
    response.SignHMAC("secret-key");

    VerifyHmac(response, "secret-key").Should().BeTrue();
}

[Test]
public void VerifyHmac_WithWrongKey_ReturnsFalse()
{
    var response = new GetExonymsResponse { DefaultName = "Test" };
    response.SignHMAC("secret-key");

    VerifyHmac(response, "wrong-key").Should().BeFalse();
}
```

### Integration Tests

- Test HMAC signing and verification
- Test HTTPS configuration
- Test secrets loading

### Security Scanning

```bash
# SAST (Static Application Security Testing)
dotnet build --configuration Release

# DAST (Dynamic Application Security Testing)
# Run OWASP ZAP against deployed instance

# Dependency scanning
trivy image exonyms-api
```

## Incident Response

### HMAC Key Compromise

1. **Rotate key immediately**
2. **Update configuration** on all servers
3. **Notify clients** to update their keys
4. **Audit logs** for suspicious activity

### External API Compromise

1. **Disable affected API**
2. **Rotate credentials**
3. **Audit access logs**
4. **Notify affected parties**

### Data Breach

1. **Contain the breach**
2. **Assess impact**
3. **Notify affected parties**
4. **Document and learn**

## Security Checklist

### Development

- [ ] No secrets in source code
- [ ] No secrets in Git history
- [ ] GeoNames usernames moved to configuration
- [ ] HTTPS enabled in development
- [ ] Sensitive data masked in logs

### Production

- [ ] Strong HMAC signing key (32+ chars)
- [ ] HTTPS enforced
- [ ] Secrets in secrets manager
- [ ] Regular security audits
- [ ] Incident response plan
- [ ] Monitoring and alerting

### CI/CD

- [ ] Dependency vulnerability scanning
- [ ] SAST scanning
- [ ] Secrets scanning in commits
- [ ] Automated security tests

## Related Documentation

- [Architecture](../architecture.md)
- [Configuration](../configuration.md)
- [Error Handling](../error-handling.md)
- [Build and Deployment](../build-and-deployment.md)
- [Change Guide](../change-guide.md)