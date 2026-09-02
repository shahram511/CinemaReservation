# Cinema Reservation API

## Overview
A robust, enterprise-grade backend system for managing cinema ticket reservations, movie schedules, and user engagement. Designed to handle high-concurrency seat bookings, this application prevents double-booking using optimistic concurrency control. It features a self-healing database architecture that automatically runs migrations and seeds essential data (like theater seats) upon initialization.

## Architecture & Tech Stack
This project strictly follows the **Clean Architecture** pattern, dividing concerns into `API`, `Core` (Domain/Application logic), and `Infrastructure` layers to ensure high maintainability and testability.

* **Framework:** C# .NET 10 (ASP.NET Core Web API)
* **Polyglot Persistence:** 
  * **PostgreSQL:** Relational data (Users, Movies, Reservations, Seats) managed via Entity Framework Core.
  * **MongoDB:** Unstructured document data (User Comments/Reviews).
* **Infrastructure:** Docker & Docker Compose (Multi-stage builds).
* **Documentation:** Scalar and OpenAPI.

## Key Features
* **Secure Authentication:** JWT-based identity management with standard claim mappings and Role-Based Access Control (RBAC).
* **Concurrency Management:** Built-in safeguards using EF Core optimistic concurrency to prevent seat collisions and overbooking.
* **Centralized Error Handling:** An `IExceptionHandler` middleware intercepts domain errors and database faults, returning clean, standardized HTTP Problem Details.
* **Auto-Seeding & Migrations:** The application automatically applies PostgreSQL schema updates and dynamically generates theater seat topology (Rows A-F, 1-10) on boot.

## Getting Started (Docker)
The entire application environment, including the database infrastructure, is containerized. No manual SDK installations or database configurations are required.

1. **Clone the repository:** Ensure you are in the root directory containing the `.sln` and `docker-compose.yml` files.
2. **Build and Run:** Execute the following command to provision the databases and launch the API:
   ```bash
   docker compose up --build -d