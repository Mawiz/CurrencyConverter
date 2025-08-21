#region Completed
# Currency Conversion API

## Objective
The Currency Conversion API is designed and implemented using **C#** and **ASP.NET Core** to provide a robust, scalable, and maintainable solution for currency conversion. The API ensures **high performance, security, and resilience** while allowing extensibility for future enhancements.

---

## Features / Endpoints

### 1. Retrieve Latest Exchange Rates
- Fetch the latest exchange rates for a specific base currency (e.g., EUR).
- Example endpoint: `GET /api/exchangerates/latest?base=EUR`

### 2. Currency Conversion
- Convert amounts between different currencies.
- Currencies **TRY, PLN, THB, and MXN** are excluded; requests involving these return a **Bad Request** response.
- Example endpoint: `POST /api/exchangerates/convert`

### 3. Historical Exchange Rates with Pagination
- Retrieve historical exchange rates for a given date range with pagination.
- Example: `GET /api/exchangerates/history?base=EUR&start=2020-01-01&end=2020-01-31&page=1&pageSize=20`

---

## Architecture & Design Considerations

### Resilience & Performance
- **Caching** implemented to minimize direct calls to the Frankfurter API.
- **Retry policies** with exponential backoff for handling intermittent API failures.
- **Circuit breaker** to gracefully handle API outages.

### Extensibility & Maintainability
- **Dependency Injection** for service abstractions.
- **Factory Pattern** to dynamically select the currency provider based on requests.
- Designed to support **multiple exchange rate providers** in the future.

### Security & Access Control
- **JWT Authentication** for secure access.
- **Role-Based Access Control (RBAC)** for endpoints.
- **API throttling** to prevent abuse.

### Logging & Monitoring
- Logs include:
  - Client IP
  - ClientId from JWT
  - HTTP Method & Target Endpoint
  - Response Code & Response Time
- Correlation of requests with the Frankfurter API.
- Structured logging using **Serilog**.
- Distributed tracing implemented with **OpenTelemetry**.

---

## Testing & Quality Assurance
- **Unit test coverage**: 90%+
- Integration tests to verify API interactions.
- Test coverage reports available.

---

## Deployment & Scalability
- Supports deployment in **Dev, Test, Prod** environments.
- Designed for **horizontal scaling** to handle high request volumes.
- Implements **API versioning** for backward compatibility.

---

## Evaluation Criteria Covered
-  Solution Architecture: Extensibility, Maintainability, Resilience
-  Code Quality: SOLID principles, Design Patterns, Dependency Injection
-  Security & Performance: JWT, Rate Limiting, Caching
-  Observability: Structured Logging, Distributed Tracing
-  Testing & CI/CD Readiness: Coverage, Automation, Deployment Strategy

---
#endregion



## Setup Instructions
1. Clone the repository:
   
   git clone https://github.com/Mawiz/CurrencyConverter.git

2. Install dependencies:
   bash
   dotnet restore

## Possible Future Enhancements

1. Multiple Exchange Rate Providers

Integrate additional providers besides Frankfurter to ensure redundancy and better coverage.

Dynamically switch providers based on availability or performance.

2. Real-Time Updates

Add WebSocket or SignalR support for live currency rate updates.

3. Advanced Historical Data Analysis

Provide features like trend charts, averages, or comparisons over custom periods.

3. Extended API Versioning & SDKs

Provide multiple API versions and client SDKs for easier integration.

Rate Prediction & AI Enhancements

Add predictive analytics or AI models to forecast currency trends.

4. Improved Pagination & Filtering

Allow flexible queries for historical rates with advanced filtering and sorting.

5. Localization & Multi-Currency Support

Support localized formatting, currency symbols, and regional decimal formats.
