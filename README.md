[![Donate](https://img.shields.io/badge/-%E2%99%A5%20Donate-%23ff69b4)](https://hmlendea.go.ro/funding)
[![Latest Release](https://img.shields.io/github/v/release/hmlendea/exonyms-api)](https://github.com/hmlendea/exonyms-api/releases/latest)
[![Build Status](https://github.com/hmlendea/exonyms-api/actions/workflows/dotnet.yml/badge.svg)](https://github.com/hmlendea/exonyms-api/actions/workflows/dotnet.yml)
[![License](https://img.shields.io/github/license/hmlendea/exonyms-api)](https://github.com/hmlendea/exonyms-api/blob/master/LICENSE)

# Exonyms API

REST API that gathers exonyms for a geographic location, transliterates names to Latin script, and returns a signed JSON response.

## 📑 Table of Contents

- [Capabilities](#capabilities)
- [Use Cases](#use-cases)
- [Usage](#usage)
- [Known Limitations](#known-limitations)
- [System Requirements](#system-requirements)
- [Installation](#installation)
	- [Manual Installation](#manual-installation)
	- [Installation from Source](#installation-from-source)
	- [Verification](#verification)
- [Configuration](#configuration)
	- [Configuration Files](#configuration-files)
	- [Settings](#settings)
	- [Precedence](#precedence)
	- [Secret Management](#secret-management)
- [Compatibility](#compatibility)
- [Integrations](#integrations)
- [Privacy and Data](#privacy-and-data)
- [Development](#development)
	- [Requirements](#requirements)
	- [Setup](#setup)
	- [Build](#build)
	- [Run](#run)
	- [Test](#test)
	- [Release](#release)
	- [Dependencies](#dependencies)
- [GitHub Actions](#github-actions)
- [Project Structure](#project-structure)
	- [Projects and Packages](#projects-and-packages)
	- [Directories](#directories)
- [Architecture](#architecture)
- [Deployment](#deployment)
	- [Configuration and Secrets](#configuration-and-secrets)
- [Documentation](#documentation)
- [API Reference](#api-reference)
- [Versioning](#versioning)
- [Troubleshooting](#troubleshooting)
- [Contributing](#contributing)
- [Acknowledgements](#acknowledgements)
- [Security](#security)
- [Project Engagement](#project-engagement)
- [License](#license)

## ✨ Capabilities

- Retrieves alternate names for locations identified by GeoNames and/or WikiData identifiers.
- Aggregates and processes names from GeoNames and WikiData.
- Transliterate names to Latin script through the configured transliteration service.
- Normalise administrative names, construct historical forms, and apply language fallbacks.
- Return HMAC-SHA256-signed responses containing the default name, processed names, and count.

## 🎯 Use Cases

- **API consumers:** Retrieve exonyms for a location through a single REST endpoint.
- **Self-hosted operators:** Deploy the service with controlled configuration, logging, and network exposure.
- **Maintainers:** Build, test, and release the .NET service from the repository.

## 🚀 Usage

Start the service locally, then request exonyms with at least one location identifier:

```bash
dotnet run --project ExonymsAPI/ExonymsAPI.csproj
curl --request GET --location 'http://localhost:5000/Exonyms?wikiDataId=Q310350'
```

The response contains the default name, a language-keyed `names` object, an entry count, and an HMAC signature.

## ⚠️ Known Limitations

- Each request depends on the availability and response formats of the GeoNames, WikiData, and transliteration services.
- The service has no persistent storage or cache; external data is fetched for each request.
- Health-check endpoints are not implemented.

## 🖥️ System Requirements

| Component | Minimum | Recommended |
|-----------|---------|-------------|
| .NET runtime or SDK | 10.0 | 10.0 |

## 📦 Installation

[![Obtain it from GitHub](https://raw.githubusercontent.com/hmlendea/readme-assets/master/badges/stores/github.png)](https://github.com/hmlendea/exonyms-api/releases)

### Manual Installation

Download the archive for the target platform from the [latest GitHub release](https://github.com/hmlendea/exonyms-api/releases/latest), extract it, configure the service, and run the resulting `ExonymsAPI.dll` with the .NET 10 runtime.

Published archives are available for Linux ARM, Linux ARM64, Linux x64, macOS ARM64, macOS x64, Windows ARM64, and Windows x64.

### Installation from Source

```bash
git clone https://github.com/hmlendea/exonyms-api.git
cd exonyms-api
dotnet restore
dotnet build ExonymsAPI.sln
```

### Verification

```bash
curl --request GET --location 'http://localhost:5000/Exonyms?wikiDataId=Q310350'
```

## ⚙️ Configuration

Configuration is loaded from JSON files, environment variables, command-line arguments, and development user secrets. Required settings include the transliteration API base URL and an HMAC signing key.

### Configuration Files

| File | Scope | Purpose |
|------|-------|---------|
| `ExonymsAPI/appsettings.json` | Base | Defines transliteration, security, and logging settings. |
| `ExonymsAPI/appsettings.Development.json` | Development | Provides development logging and transliteration overrides. |

### Settings

The subsequent settings are recognised:

| Section | Key | Type | Default | Required | Description |
|---------|-----|------|---------|----------|-------------|
| `TransliterationSettings` | `TransliterationApiBaseUrl` | `string` | — | Yes | Base URL of the transliteration API. |
| `SecuritySettings` | `HmacSigningKey` | `string` | — | Yes | Secret used to sign successful responses with HMAC-SHA256. |
| `NuciLoggerSettings` | `LogFilePath` | `string` | `logfile.log` | No | Path for the log file. |
| `NuciLoggerSettings` | `IsFileOutputEnabled` | `boolean` | `true` | No | Enables file logging. |

### Precedence

Configuration sources are applied from lowest to highest precedence in this order: `appsettings.json`, `appsettings.Development.json`, environment variables, command-line arguments, and development user secrets.

### Secret Management

Do not commit real secrets. Set `SecuritySettings:HmacSigningKey` through an environment variable, development user secrets, or a deployment secret store. The equivalent environment variable is `EXONYMSAPI_SecuritySettings__HmacSigningKey`.

## 🧩 Compatibility

| Component | Supported Versions | Notes |
|-----------|--------------------|-------|
| .NET | 10.0 | Required for building and running the service. |
| Release platforms | Linux ARM, Linux ARM64, Linux x64, macOS ARM64, macOS x64, Windows ARM64, Windows x64 | Platform archives are published with GitHub releases. |

## 🔌 Integrations

| Integration | Compatibility | Purpose | Required |
|-------------|---------------|---------|----------|
| GeoNames API | GeoNames web services | Supplies alternate place names and geographic data. | Yes |
| WikiData API | WikiData EntityData JSON | Supplies labels, aliases, and sitelinks. | Yes |
| Transliteration API | Configured HTTP endpoint | Converts names to Latin script. | Yes |

## 🛡️ Privacy and Data

Exonyms API processes location identifiers and external geographic-name data to produce signed exonym responses. It does not request personal data, persist application data, or send data to the project maintainers.

For the detailed description of how the application handles privacy and personal data, see [PRIVACY.md](./PRIVACY.md).

## 🛠️ Development

### Requirements

- [.NET 10.0 SDK](https://dotnet.microsoft.com/download/dotnet/10.0)

### Setup

```bash
git clone https://github.com/hmlendea/exonyms-api.git
cd exonyms-api
dotnet restore
```

Configure the required settings before running the service.

### Build

```bash
dotnet build ExonymsAPI.sln
```

### Run

```bash
dotnet run --project ExonymsAPI/ExonymsAPI.csproj
```

The local service is documented at `http://localhost:5000`.

### Test

```bash
dotnet test ExonymsAPI.sln
```

### Release

The repository includes `release.sh`, which delegates to the upstream deployment script used by the project maintainer.

```bash
bash ./release.sh 3.1.1
```

This script downloads and executes an external release helper from `https://raw.githubusercontent.com/hmlendea/deployment-scripts/master/release/dotnet/10.0.sh`.

**Note:** Piping into `bash` is an intensely controversial topic. Please review any external scripts before running them in your environment!

### Dependencies

| Package | Version | Scope | Purpose |
|---------|---------|-------|---------|
| `Newtonsoft.Json` | `13.0.4` | Runtime | JSON processing. |
| `NuciAPI` | `3.3.0` | Runtime | API request and response primitives. |
| `NuciAPI.Client` | `1.2.0` | Runtime | External API client support. |
| `NuciAPI.Controllers` | `2.2.0` | Runtime | Controller and response handling. |
| `NuciAPI.Middleware` | `1.3.0` | Runtime | Request logging and exception handling middleware. |
| `NuciLog` | `1.1.2` | Runtime | Application logging. |
| `NuciLog.Core` | `2.5.0` | Runtime | Logging configuration and implementation. |
| `NuciSecurity.HMAC` | `4.1.2` | Runtime | HMAC response signing. |
| `NuciWeb.HTTP` | `1.2.0` | Runtime | HTTP utilities. |
| `NUnit` | `4.5.1` | Development | Unit testing framework. |
| `Moq` | `4.20.72` | Development | Test dependency mocking. |

## ⚙️ GitHub Actions

| Workflow | Purpose | What it does |
|----------|---------|--------------|
| [ .NET ](https://github.com/hmlendea/exonyms-api/blob/master/.github/workflows/dotnet.yml) | Validate .NET changes on pushes and pull requests to `master`. | Checks out the repository, installs .NET 10, restores dependencies, builds the solution, and runs tests. |

## 🗂️ Project Structure

The repository contains the production API, its unit tests, and supporting documentation.

### Projects and Packages

| Project | Type | Purpose |
|---------|------|---------|
| `ExonymsAPI/` | .NET web application | Hosts the REST API and application services. |
| `ExonymsAPI.UnitTests/` | .NET test project | Tests service, processor, and gatherer behaviour. |

### Directories

| Directory | Purpose |
|-----------|---------|
| `ExonymsAPI/API/` | Controllers, requests, and responses. |
| `ExonymsAPI/Client/` | External transliteration API client. |
| `ExonymsAPI/Service/` | Exonym gathering, processing, and domain models. |
| `docs/` | Architecture, API, configuration, testing, and operational documentation. |

## 🏗️ Architecture

Exonyms API is a layered REST service that gathers exonyms from GeoNames and WikiData, processes names through transliteration and normalisation pipelines, and returns signed responses.

See the [architecture documentation](./ARCHITECTURE.md) for the system context, principal components, runtime flows, ownership boundaries, dependencies, constraints, and extension points.

## 🚢 Deployment

The service can be deployed from a published release archive or as a self-hosted .NET application. Configure the required transliteration endpoint and HMAC signing key, expose the selected HTTP or HTTPS URL, and place the instance behind the network controls appropriate for its environment.

### Configuration and Secrets

Set production secrets through environment variables or a secret manager. Protect the HMAC signing key and any upstream API credentials, configure HTTPS, and restrict network exposure according to the deployment environment.

## 📚 Documentation

The repository contains detailed documentation for architecture, configuration, integrations, testing, deployment, and API behaviour.

| Resource | Description |
|----------|-------------|
| [Documentation index](./docs/INDEX.md) | Catalogue of repository documentation. |
| [Architecture](./ARCHITECTURE.md) | System context, components, runtime flows, and boundaries. |
| [Configuration](./docs/configuration.md) | Configuration schema, sources, precedence, and secrets guidance. |
| [Build and deployment](./docs/build-and-deployment.md) | Build, release, and deployment procedures. |
| [Testing](./docs/testing.md) | Test organisation and execution guidance. |

## 🌐 API Reference

See the [Exonyms Controller API reference](./docs/api-reference/exonyms-controller.md) for the endpoint, query parameters, response fields, error responses, and HMAC signing details.

## 🏷️ Versioning

Releases use Semantic Versioning in `MAJOR.MINOR.PATCH` form, as documented in the build and deployment guide.

## 🩺 Troubleshooting

| Symptom | Probable Cause | Resolution |
|---------|----------------|------------|
| The service fails when it starts or handles a request. | A required setting is missing or invalid. | Configure `TransliterationSettings:TransliterationApiBaseUrl` and `SecuritySettings:HmacSigningKey`, then restart the service. |
| Exonym retrieval fails or is incomplete. | An external GeoNames, WikiData, or transliteration service is unavailable or returned an unexpected response. | Check upstream availability and the application logs, then retry the request. |

## 🤝 Contributing

You are welcome to submit any suggestion, feedback, or modification to this project.

When doing so, please:
- Maintain cross-platform compatibility
- Preserve the existing public contract unless a breaking change is intentional
- Submit focused pull requests that conform to the existing code style
- Maintain your branch synchronised with `master`
- Revise the documentation when functionality changes
- Properly test all modifications, including edge cases and error conditions
- Add tests for additional or modified functionality
- Raise a new [issue](https://github.com/hmlendea/exonyms-api/issues) for problems or suggestions

See the [contributing guidelines](./CONTRIBUTING.md) for details on how to report issues, suggest enhancements, and submit changes.

## 🙏 Acknowledgements

- [GeoNames](https://www.geonames.org/) for geographic names and related data.
- [WikiData](https://www.wikidata.org/) for labels, aliases, and sitelinks.
- The configured transliteration service for Latin-script name conversion.

## 🔒 Security

Exonyms API follows the security practices described in its repository policy.

For information on reporting security vulnerabilities, see [SECURITY.md](./SECURITY.md).

## 💝 Project Engagement

Discovered a problem or have a suggestion? [Open an issue](https://github.com/hmlendea/exonyms-api/issues)!

If you find this project useful, consider [funding it](https://hmlendea.go.ro/funding) or starring ⭐️ it on GitHub!

[![Donate](https://raw.githubusercontent.com/hmlendea/readme-assets/master/donate_generic.png)](https://hmlendea.go.ro/funding)

## 📄 License

This project is being distributed under the `GNU General Public License v3.0` or later.
See [LICENSE](./LICENSE) for further information.
