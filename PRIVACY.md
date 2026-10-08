# Privacy and Personal Data

This document describes how the Exonyms API handles personal data. It covers the application behaviour and verified integrations described below. Where the software is self-hosted, the instance operator may have separate responsibilities described below.

**Information reviewed:** 2026-10-08

## 📑 Table of Contents

- [What This Document Covers](#-what-this-document-covers)
- [Self-Hosted Deployments](#-self-hosted-deployments)
- [Data We Handle](#-data-we-handle)
- [Processing and Use](#-processing-and-use)
- [Storage, Retention, and Deletion](#-storage-retention-and-deletion)
- [External Processing and Integrations](#-external-processing-and-integrations)
- [User Controls and Requests](#-user-controls-and-requests)
- [International Transfers](#-international-transfers)
- [Children](#-children)
- [Data Protection and Security](#-data-protection-and-security)
- [Document Changes](#-document-changes)
- [Contact](#-contact)

## 🔎 What This Document Covers

This document describes how Exonyms API at https://github.com/hmlendea/exonyms-api handles personal data. It covers the application behaviour and verified integrations described below. Where the software is self-hosted, the instance operator may have separate responsibilities described below.

## 🏠 Self-Hosted Deployments

This document covers both the project maintainers and self-hosted instance operators. The software is distributed as source code and can be deployed and operated by users or organisations.

The instance operator controls their instance's configuration, local storage, logs, backups, access controls, retention, and request handling. The project maintainers do not operate any hosted instance and do not receive data from self-hosted deployments.

No data is sent from a self-hosted instance to project maintainers. There is no telemetry, update checks, crash reports, email, authentication, reverse-proxy, object-storage, or monitoring integration built into the application.

## 📥 Data We Handle

### Data Provided to the Application

- **Location identifiers**: The endpoint accepts `geoNamesId` and/or `wikiDataId` query parameters. These are identifiers for geographic entities, not personal data.
- **No personal data is requested** from API consumers. No registration, authentication, or account is required.

### Data Generated or Collected by the Application

- **No personal data is generated or collected automatically.** The application is stateless: it does not store requests, responses, or logs containing request data.
- **Server logs**: NuciAPI request logging records HTTP request metadata (method, path, status code) for operational purposes. Log retention is controlled by the instance operator.

### Data Received from Integrations

- **GeoNames**: Alternate place names, coordinates, and administrative data for geographic entities.
- **WikiData**: Labels, aliases, and sitelinks for geographic entities.
- **Transliteration API**: Transliterated text for supported languages.
- None of these sources are queried with personal data; queries use geographic entity identifiers only.

## 🧭 Processing and Use

The application processes the data described above for these verified functions:
- **Exonym aggregation** — Location identifiers (`geoNamesId`, `wikiDataId`) — Fetches alternate names from GeoNames and WikiData.
- **Name processing** — Alternate names — Transliterates names to Latin script, normalises administrative suffixes, constructs historical forms, and applies language fallbacks.
- **Response signing** — Response body — Applies HMAC-SHA256 signing for response integrity.
- **Request logging** — HTTP request metadata — Records request metadata for operational monitoring.

## 🗄️ Storage, Retention, and Deletion

- **No persistent storage**: The application does not persist any data. All data is processed in memory and discarded after the response is returned.
- **No caching**: There is no in-process or external cache. Each request fetches data fresh from external sources.
- **Logs**: Any server logs are retained according to the instance operator's configuration. The project does not control log retention.
- **Backups**: No backups are created by the application.

## 🔗 External Processing and Integrations

The application forwards requests to the following verified external services:

| Service or integration | Purpose | Data involved | Configuration or documentation |
|-----------------------|---------|---------------|--------------------------------|
| GeoNames API | Fetch alternate place names | Geographic entity identifier (`geonameId`), rotating username | https://www.geonames.org/export/web-services.html |
| WikiData API | Fetch labels and sitelinks | Geographic entity identifier (`wikiDataId`) | https://www.wikidata.org/wiki/Wikidata:Main_Page |
| Transliteration API | Transliterate names to Latin script | Source name text and language code | Configured via `TransliterationSettings` |

The application has no built-in external data transfer to project maintainers. All external calls originate from the instance (self-hosted) or from the API consumer's request.

## ⚙️ User Controls and Requests

The following documented controls or request procedures are available:
- **No user accounts**: There are no accounts, so no data subject access, correction, or deletion requests are applicable.
- **Self-hosted operators**: Operators control their instance's logs, configuration, and network exposure. Operators may disable request logging by removing the `UseNuciApiRequestLogging()` middleware call.

## 🌍 International Transfers

The application itself does not transfer data across borders; it forwards requests to external APIs. GeoNames, WikiData, and the Transliteration API may process requests from servers located in various countries. For self-hosted deployments, the deployment location is controlled by the instance operator.

## 🧒 Children

This service is not directed to children. There are no child accounts or age-related controls.

## 🛡️ Data Protection and Security

- **HTTPS**: The application enforces HTTPS redirection in production (`UseHttpsRedirection`).
- **Response signing**: All responses are HMAC-signed to detect tampering.
- **No secrets in code**: The HMAC signing key, transliteration API endpoint, and GeoNames usernames are configured via environment variables or configuration files, not hardcoded.
- **Self-hosted operators** are responsible for: keeping the application updated, protecting secrets (HMAC key, API credentials), configuring access controls and network exposure, managing backups, and protecting logs. The project does not promise absolute security.

## 🔄 Document Changes

Update this document when application data flows, storage, integrations, or deployment responsibilities change. The current version is published at https://github.com/hmlendea/exonyms-api/blob/main/PRIVACY.md.

## 📬 Contact

For questions about application data handling, contact the project maintainers. For a self-hosted instance, contact the instance operator, unless the project explicitly handles the request. Include the deployment URL or request details, if any; do not send passwords, access tokens, or other secrets.
