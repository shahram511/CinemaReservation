# Cinema Reservation API

## Overview
A robust, enterprise-grade backend system for managing cinema ticket reservations, movie schedules, and user engagement. Designed to handle high-concurrency seat bookings, this application prevents double-booking using optimistic concurrency control. It features a self-healing database architecture that automatically runs migrations and seeds essential data (like theater seats) upon initialization.

---

## Architecture & Tech Stack
This project strictly follows the **Clean Architecture** pattern, dividing concerns into `API`, `Core` (Domain/Application logic), and `Infrastructure` layers to ensure high maintainability and testability.

*   **Framework:** C# .NET 10 (ASP.NET Core Web API)
*   **Polyglot Persistence:** 
    *   **PostgreSQL:** Relational data (Users, Movies, Reservations, Seats) managed via Entity Framework Core.
    *   **MongoDB:** Unstructured document data (User Comments/Reviews).
*   **Infrastructure:** Docker & Docker Compose (Multi-stage builds).
*   **Documentation:** Scalar and OpenAPI.

---

## Key Features
*   **Secure Authentication:** JWT-based identity management with standard claim mappings and Role-Based Access Control (RBAC).
*   **Concurrency Management:** Built-in safeguards using EF Core optimistic concurrency to prevent seat collisions and overbooking.
*   **Centralized Error Handling:** An `IExceptionHandler` middleware intercepts domain errors and database faults, returning clean, standardized HTTP Problem Details.
*   **Auto-Seeding & Migrations:** The application automatically applies PostgreSQL schema updates and dynamically generates theater seat topology (Rows A-F, 1-10) on boot.

---

## Getting Started (Docker)
The entire application environment, including the database infrastructure, is containerized. No manual SDK installations or database configurations are required.

1.  **Clone the repository:** Ensure you are in the root directory containing the `.sln` and `docker-compose.yml` files.
2.  **Build and Run:** Execute the following command to provision the databases and launch the API:
    `docker compose up --build -d`
3.  **Access the API:** Navigate to `http://localhost:8080/scalar/v1` in your browser to view the interactive endpoints.
4.  **Local Debugging:** You can also run the API directly via Visual Studio/VS Code (which will bind to a local port like 7228) while keeping the database containers running in the background.

---
## Testing Infrastructure

The testing suite is designed for zero-friction CI/CD pipeline integration and absolute environment isolation. 

*   **Environment-Agnostic Execution:** The End-to-End (E2E) and integration test suites utilize **Testcontainers**. The framework dynamically provisions temporary PostgreSQL and MongoDB instances directly via the Docker daemon for each test run.
*   **Deterministic State:** **Respawn** is used to rapidly clear transactional data between test methods, ensuring a clean slate without dropping critical seeded infrastructure (e.g., Admin accounts and physical Seat maps).
*   **Authentic Security Validation:** E2E tests strictly bypass mock authentication handlers. The test suite issues real HTTP requests, generates real JWTs, and validates precise `UserId` claim extractions against actual PostgreSQL Foreign Key constraints.

**Running the Test Suite:**
Execute the following command in the root directory. Testcontainers will automatically pull the required database images, map available network ports, execute the suite, and tear down the temporary containers.

```bash
dotnet test

---


##  Project URL

** https://roadmap.sh/projects/movie-reservation-system
