# Notification Platform

> **Status:** In progress — core functionality is implemented, additional features are planned.

![CI](https://github.com/Gregor-an/notification-platform/actions/workflows/dotnet-ci.yml/badge.svg)
![CodeQL](https://github.com/Gregor-an/notification-platform/actions/workflows/codeql.yml/badge.svg)

A .NET backend service for processing, delivering, and tracking notifications.

## Overview

Notification Platform is a portfolio project that simulates a small production-style notification service.

The system accepts notification requests through a REST API, stores them in a database, and processes them asynchronously in the background. The current implementation is focused on the **Email** channel, while the architecture is prepared for future support of **Sms** and **Push** notifications.

This project is intended to demonstrate backend engineering practices such as clean architecture, background processing, validation, testing, and maintainable service design.

## Architecture

The solution follows a layered architecture with clear separation of responsibilities:

```text
src/
├── API                  # ASP.NET Core Web API
├── Application          # Use cases, commands, queries, abstractions
├── Contracts            # Request/response DTOs
├── Domain               # Entities, value objects, enums, business rules
├── Infrastructure       # EF Core, repositories, provider implementations
├── Orchestrator         # Background worker for pending notifications
└── Web                  # Planned UI

tests/
├── UnitTests
└── IntegrationTests
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

- Create notification API
- Persistent storage with EF Core
- Background processing worker
- Email notification flow
- Delivery attempt tracking
- Request validation with FluentValidation
- Unit and integration tests
- CI and static analysis with GitHub Actions

The domain model already includes support for:

- `Email`
- `Sms`
- `Push`

At the moment, **Sms** and **Push** are reserved for future extension and are not fully implemented yet.

## Tech Stack

- C#
- .NET 8
- ASP.NET Core Web API
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
dotnet ef database update --project src/Infrastructure --startup-project src/API
```

### Run the API

```bash
dotnet run --project src/API
```

### Run the background worker

```bash
dotnet run --project src/Orchestrator
```

### Run tests

```bash
dotnet test
```

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
- [ ] Notification details and listing endpoints
- [ ] Retry policy with configurable max attempts
- [ ] Simple UI dashboard
- [ ] SMS provider
- [ ] Push provider

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

- LinkedIn: https://linkedin.com/in/yehorpoliakov
- Email: poliakovyehor@gmail.com