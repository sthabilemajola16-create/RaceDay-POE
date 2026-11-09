RACEDAY PROG6212 POE — START HERE

1. Open RaceDay.sln in Visual Studio 2022.
2. Ensure .NET 8 SDK and SQL Server/LocalDB are installed.
3. Execute docs/RaceDay.sql in SSMS.
4. Confirm src/RaceDay.Api/appsettings.json connection string.
5. Start RaceDay.Api first; test Swagger at /swagger.
6. Set RaceDay.Web Api:BaseUrl to the API URL, then start RaceDay.Web.
7. Run tests/RaceDay.Api.Tests.
8. Review docs/PART3_IMPLEMENTATION_AND_VERIFICATION.md and docs/DEMO_VIDEO_SCRIPT.md.

IMPORTANT BEFORE SUBMISSION:
- Fill your own name and student number into README.md and any required cover pages. The uploaded Part 1 PDF identified a different student and has been excluded from this package.
- The MVC completion is a baseline integration: public event browsing/details, registration/login and organiser event creation are included. Full category management, participant enrolment/results/profile screens, Azure Blob uploads and real CI/Docker verification still need to be implemented/configured and demonstrated before claiming complete rubric coverage.
- Capture genuine screenshots and a real demonstration video on your own machine. No fabricated evidence is included.
- Run dotnet build and dotnet test locally; this environment does not have the .NET SDK, so compilation and tests have not been verified here.
