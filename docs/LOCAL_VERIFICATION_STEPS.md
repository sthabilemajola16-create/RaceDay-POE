# Local verification steps

## Visual Studio
- Install .NET 8 SDK and SQL Server.
- Open the solution and restore NuGet packages.
- Configure the SQL Server connection string in `src/RaceDay.Api/appsettings.json`.
- Set multiple startup projects: `RaceDay.Api` and `RaceDay.Web`.
- Start both projects. Confirm API Swagger and MVC home page load.

## API smoke tests
1. Swagger `POST /api/auth/register` with an Organiser.
2. Login using `POST /api/auth/login`.
3. Create an event, then add at least one category.
4. Register a Participant in a separate browser/private window.
5. Login as Participant and enrol using the event and category IDs.
6. Login as Organiser and inspect the event enrolment list.
7. Capture a finish time and position; login as Participant and check `/api/results/me`.
8. Try the same organiser-only endpoints as Participant and confirm HTTP 403.
9. Try protected endpoints without a session and confirm HTTP 401.

## Automated checks
Run `dotnet test RaceDay.sln`. Push to GitHub and inspect Actions for a green build. Record actual test output and CI evidence; never substitute a mock screenshot.

## Docker
Copy `.env.example` to `.env`, replace the password, then run `docker compose up --build`. Verify the web app at port 8080 and Swagger at port 5188.
