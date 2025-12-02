# CountriesApp

A .NET 9 Web API for managing countries and cities with Auth0 authentication, SQL Server, and Redis caching.

## Prerequisites

- [.NET 9 SDK](https://dotnet.microsoft.com/download/dotnet/9.0)
- [Docker Desktop](https://www.docker.com/products/docker-desktop)
- Auth0 account (for authentication)

## Quick Start with Docker

### 1. Start Infrastructure (SQL Server + Redis)

```bash
docker-compose up -d
```

This starts:
- **SQL Server** (Azure SQL Edge) on `localhost:1433`
- **Redis** on `localhost:6379`

### 2. Configure Auth0

Update `CountriesApp.Api/appsettings.json`:

```json
"Auth0": {
  "Domain": "your-domain.auth0.com",
  "Audience": "your-api-audience"
}
```

### 3. Run Database Migrations

```bash
cd CountriesApp.Api
dotnet ef database update --project ../CountriesApp.Infrastructure
```

### 4. Run the Application

```bash
dotnet run --project CountriesApp.Api
```

API available at: `http://localhost:5267` | Swagger: `http://localhost:5267/swagger`

### 5. Stop Infrastructure

```bash
docker-compose down
```

## Running Tests

### Unit Tests (Fast - No Docker Required)
```bash
dotnet test CountriesApp.Tests.Unit
```

### Integration Tests (Requires Docker)
```bash
dotnet test CountriesApp.Tests.Integration
```

### Acceptance Tests (Requires Docker)
```bash
dotnet test CountriesApp.Tests.Acceptance
```

### Run All Tests
```bash
dotnet test
```

## Project Structure

```
CountriesApp/
├── CountriesApp.Api/              # Web API & Endpoints
├── CountriesApp.Application/      # Business Logic & Services
├── CountriesApp.Domain/           # Entities & Interfaces
├── CountriesApp.Infrastructure/   # Data Access & Repositories
├── CountriesApp.Tests.Unit/       # Unit Tests (Moq)
├── CountriesApp.Tests.Integration/ # Integration Tests (Testcontainers)
├── CountriesApp.Tests.Acceptance/ # Acceptance Tests (E2E)
├── docker-compose.yml             # SQL Server + Redis
└── Dockerfile                     # API Container
```

## API Endpoints

All endpoints require Auth0 authentication and are subject to rate limiting.

### Rate Limiting

The API implements rate limiting to protect against abuse:

- **Limit**: 100 requests per 60 seconds per user/IP
- **Queue**: Up to 10 additional requests can be queued
- **Response**: HTTP 429 (Too Many Requests) when limit exceeded
- **Configuration**: Adjustable in `appsettings.json` under `RateLimiting` section

```json
"RateLimiting": {
  "PermitLimit": 100,
  "Window": 60,
  "QueueLimit": 10
}
```

### Countries
- `GET /countries` - List countries (with pagination & search)
- `GET /countries/{id}` - Get country by ID
- `POST /countries` - Create country
- `PUT /countries/{id}` - Update country
- `DELETE /countries/{id}` - Delete country

### Cities
- `GET /cities` - List cities (with pagination, search & country filter)
- `GET /cities/{id}` - Get city by ID
- `POST /cities` - Create city
- `PUT /cities/{id}` - Update city
- `DELETE /cities/{id}` - Delete city

## Technologies

- **.NET 9** - Web API
- **Entity Framework Core 9** - ORM
- **SQL Server** (Azure SQL Edge) - Database
- **Redis** - Distributed Cache
- **Auth0** - JWT Authentication
- **AutoMapper** - Object Mapping
- **xUnit** - Testing Framework
- **Moq** - Mocking
- **FluentAssertions** - Test Assertions
- **Testcontainers** - Integration Testing

## Development

### Database Migrations

Create a new migration:
```bash
dotnet ef migrations add MigrationName --project CountriesApp.Infrastructure --startup-project CountriesApp.Api
```

Apply migrations:
```bash
dotnet ef database update --project CountriesApp.Infrastructure --startup-project CountriesApp.Api
```

### Docker Commands

Restart services:
```bash
docker-compose restart
```

Remove all data:
```bash
docker-compose down -v
```

## Configuration

### Local Development (`appsettings.json`)
- SQL Server: `localhost:1433`
- Redis: `localhost:6379`

### Docker Environment (`appsettings.Docker.json`)
- SQL Server: `sqlserver:1433`
- Redis: `redis:6379`


