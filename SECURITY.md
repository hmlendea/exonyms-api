# Security Policy

This policy defines how security vulnerabilities are reported, handled, and disclosed for the Exonyms API project. It covers supported versions, in-scope categories, and coordinated disclosure expectations.

## 📑 Table of Contents

- [Supported Versions](#-supported-versions)
- [Reporting a Vulnerability](#-reporting-a-vulnerability)
- [Scope](#-scope)
- [Disclosure Policy](#-disclosure-policy)
- [Safe Harbour](#-safe-harbour)

## 🛡️ Supported Versions

Use this table to indicate which project versions currently receive security maintenance.

| Version | Distribution Channel | Supported |
|---------|--------------------|-----------|
| Latest version | GitHub Releases | ✅ |
| Latest version | Source code (master branch) | ✅ |
| Preceding versions | Any distribution channel | ❌ |

## 🚨 Reporting a Vulnerability

Please do not disclose suspected vulnerabilities publicly before maintainers have had an opportunity to validate and remediate them.

To report a vulnerability:
- [GitHub Security Advisories](https://github.com/hmlendea/exonyms-api/security/advisories)
- Contact the maintainers directly

## 📌 Scope

The subsequent report categories are in scope for this repository:
- **Application vulnerabilities**: Bugs, misconfigurations, or logic errors in the Exonyms API source code that could lead to data exposure, denial of service, or unauthorised access.
- **Dependency vulnerabilities**: Security issues in third-party packages (NuGet dependencies) used by the project.
- **Configuration vulnerabilities**: Misconfigurations in the deployment environment that affect the application's security posture (e.g., missing HTTPS, weak HMAC key).

The subsequent categories are out of scope unless explicitly stated to the contrary:
- **External API vulnerabilities**: Issues in GeoNames, WikiData, or the Transliteration API that are outside the project's control.
- **Self-hosted instance misconfigurations**: Issues arising from an operator's deployment choices (e.g., network exposure, firewall rules) that are not caused by the application code.
- **Third-party distribution channels**: Issues in unofficial forks, mirrors, or package feeds not maintained by the project.

## 📢 Disclosure Policy

This project follows coordinated disclosure:
1. Vulnerabilities are investigated privately.
2. A remediation plan is prepared and validated.
3. Public disclosure is published after a fix, mitigation, or agreed risk decision is available.
4. Credit is attributed in accordance with reporter preference and project policy.

## 🧾 Safe Harbour

If your research is conducted in good faith, confined to authorised scope, and disclosed responsibly, the maintainers will not pursue action for policy-compliant activity.