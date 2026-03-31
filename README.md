# Notification Platform

> **Status:** In progress — core functionality is implemented, additional features are planned.

![CI](https://github.com/Gregor-an/notification-platform/actions/workflows/dotnet-ci.yml/badge.svg)
![CodeQL](https://github.com/Gregor-an/notification-platform/actions/workflows/codeql.yml/badge.svg)

A .NET backend service for processing, delivering, and tracking notifications.

## Overview

Notification Platform is a portfolio project that simulates a small production-style notification service.

The system accepts notification requests through a REST API, stores them in a database, and processes them asynchronously in the background. Email delivery is handled via SMTP (MailKit). SMS and Push channels are supported with mock providers, ready for real integrations.

This project is intended to demonstrate backend engineering practices such as clean architecture, background processing, validation, testing, and maintainable service design.

## Architecture

The solution follows a layered architecture with clear separation of responsibilities:

```text
Repository root
├── API/                  # ASP.NET Core Web API
├── Application/          # Use cases, commands, queries, abstractions
├── Contracts/            # Request/response DTOs
├── Domain/               # Entities, value objects, enums, business rules
├── Infrastructure/       # EF Core, repositories, provider implementations
├── Orchestrator/         # Background worker for pending notifications
├── Web/                  # Planned UI
├── UnitTests/
└── IntegrationTests/
```

## How It Works

1. A client sends a request to create a notification
2. The notification is stored with `Pending` status
3. The background worker picks up pending notifications
4. The system resolves the provider based on `ChannelType`
5. The provider attempts delivery
6. The notification status is updated based on the result

## Current Scope

The project currently includes:

- Create, list, and detail notification API endpoints
- Persistent storage with EF Core
- Background processing worker with configurable retry policy
- Email delivery via SMTP (MailKit)
- SMS and Push notification channels (mock providers)
- Delivery attempt tracking
- Request validation with FluentValidation
- Unit and integration tests
- CI and static analysis with GitHub Actions

## Tech Stack

- C#
- .NET 8
- ASP.NET Core Web API
- Blazor Server (MudBlazor)
- Entity Framework Core
- SQL Server
- FluentValidation
- xUnit
- Moq
- FluentAssertions
- GitHub Actions
- CodeQL

## Getting Started

### Prerequisites

- .NET 8 SDK
- SQL Server

### Clone the repository

```bash
git clone https://github.com/Gregor-an/notification-platform.git
cd notification-platform
```

### Apply database migrations

```bash
dotnet ef database update --project Infrastructure --startup-project API
```

### Run the API

```bash
dotnet run --project API
```

### Run the background worker

```bash
dotnet run --project Orchestrator
```

Run these commands from the repository root (the folder that contains `NotificationPlatform.sln`), or pass full paths to `--project` / `--startup-project` if you run them elsewhere.

### Run tests

```bash
dotnet test
```

## Configuration

`appsettings.json` is a **template** — it defines the shape of all config keys with safe placeholder values and is committed to source control. Sensitive values (passwords, credentials) are never stored there; they are layered on top at runtime.

| Environment | How to supply secrets |
|---|---|
| Local development | .NET User Secrets (stored outside the repo, never committed) |
| Production / CI | Environment variables (`Smtp__Password=xxx`) |

### SMTP — local setup

```bash
cd Orchestrator
dotnet user-secrets set "Smtp:Host"        "smtp.gmail.com"
dotnet user-secrets set "Smtp:Port"        "587"
dotnet user-secrets set "Smtp:Username"    "your@gmail.com"
dotnet user-secrets set "Smtp:Password"    "your-app-password"
dotnet user-secrets set "Smtp:FromAddress" "your@gmail.com"
dotnet user-secrets set "Smtp:FromName"    "Notification Platform"
```

> **Gmail note:** you cannot use your account password directly. Generate an **App Password** via Google Account → Security → 2-Step Verification → App Passwords and use that 16-character code instead.

The application reads all values through `IOptions<SmtpOptions>`. It doesn't matter whether they come from `appsettings.json`, User Secrets, or environment variables — the merged result is identical at runtime.

## API Example

### `POST /api/notifications`

Creates a new notification and queues it for delivery.

#### Request

```json
{
  "recipient": "user@example.com",
  "subject": "Welcome",
  "body": "Hello! Your account is ready.",
  "channelType": "Email",
  "priority": "Normal"
}
```

#### Response

```json
{
  "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6"
}
```

### `GET /api/notifications?page=1&pageSize=20`

Returns a paginated list of all notifications.

#### Response

```json
{
  "items": [
    {
      "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
      "recipient": "user@example.com",
      "channelType": "Email",
      "status": "Pending",
      "priority": "Normal",
      "createdUtc": "2026-03-30T10:00:00Z",
      "attemptCount": 0
    }
  ],
  "totalCount": 1,
  "page": 1,
  "pageSize": 20
}
```

### `GET /api/notifications/{id}`

Returns full details for a single notification, including all delivery attempts. Returns `404` if not found.

## Testing

The project includes:

- **Unit tests** for domain rules and processing logic
- **Integration tests** for infrastructure and API behavior

## Quality and Automation

The repository includes GitHub Actions workflows for:

- Build
- Test
- Static code analysis with CodeQL

## Roadmap

- [x] REST API for creating notifications
- [x] Persistent storage with EF Core
- [x] Background worker for pending notifications
- [x] Delivery attempt tracking
- [x] FluentValidation for incoming requests
- [x] Unit and integration tests
- [x] Notification details and listing endpoints
- [x] Retry policy with configurable max attempts
- [x] SMS provider (mock)
- [x] Push provider (mock)
- [x] Simple UI dashboard (Blazor Server + MudBlazor)

## Why This Project

This project was created as a portfolio project to demonstrate:

- backend architecture skills
- clean separation of concerns
- background processing
- delivery workflow design
- fault handling
- testability
- maintainability

## Author

**Yehor Poliakov**  
.NET Backend Developer

- LinkedIn: https://www.linkedin.com/in/yehor-poliakov-911252203
- Email: poliakovyehor@gmail.com
