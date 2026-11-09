# RaceDay POE Part 3 — Implementation and Verification

## Scope
Part 3 extends the supplied Part 2 REST API with an ASP.NET Core MVC client. The MVC application uses an HTTP client to call the API and does not contain a DbContext or direct SQL access. The interface uses a responsive purple glassmorphism theme and a custom running-track logo.

## MVC implementation
- Public home page and upcoming event list/detail pages.
- Registration and login forms call `/api/auth/register` and `/api/auth/login`.
- API session cookie is forwarded on subsequent MVC-to-API calls.
- Organiser role sees the create-event navigation and may submit event details to `/api/events`.
- Public visitors can browse events; protected API operations continue to enforce role checks.
- Responsive layout, accessible labels, focus states, semantic landmarks and mobile breakpoints.

## API and data layer retained from Part 2
The existing API controllers, EF Core context, SQL Server schema, password hashing, role/session authorization and xUnit tests are retained. Swagger remains available at `/swagger` when the API is running.

## Docker
From the repository root run `docker compose up --build`. The MVC app is exposed on `http://localhost:8080`. Start the API separately on port 5188 and configure `Api:BaseUrl` / `Api__BaseUrl` to the API address reachable from the container.

## Local setup
1. Install .NET 8 SDK, Visual Studio 2022 and SQL Server/LocalDB.
2. Execute `docs/RaceDay.sql` in SQL Server Management Studio.
3. Check `src/RaceDay.Api/appsettings.json` connection string and launch settings.
4. Start `RaceDay.Api`; open `/swagger` to inspect endpoints.
5. Start `RaceDay.Web`; open the MVC site and register separate Organiser and Participant accounts.
6. Run `dotnet test tests/RaceDay.Api.Tests/RaceDay.Api.Tests.csproj`.

## Evidence checklist — capture on the student's own machine
- API running and Swagger endpoint list.
- Registration and login for both roles.
- Participant event browsing and event details.
- Organiser event creation.
- Participant enrolment and My Enrolments / results flows where available in API.
- SQL Server tables and sample rows.
- Successful local test output.
- GitHub Actions green run.
- Docker container running and MVC page in browser.

Do not submit mock screenshots as proof. Capture actual screenshots after running the solution and replace/add them in `docs/evidence/`.

## Current implementation boundary
The provided Part 2 API does not yet expose Azure Blob upload endpoints, and the MVC pages in this completion package do not implement every advanced organiser/participant workflow described in the project README (for example full category CRUD, results capture, profile editing and upload). Azure Blob integration therefore remains a separately configurable extension and must not be claimed as verified until implemented and tested with a storage account or Azurite.
