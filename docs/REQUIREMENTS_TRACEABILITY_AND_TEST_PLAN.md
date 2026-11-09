# RaceDay POE — Requirements traceability and final verification

## Part 1 — planning and database
| Requirement in brief | Project evidence | Verification |
|---|---|---|
| ERD with at least six entities, PK/FK and cardinality | `docs/erd.md`, `docs/erd.png` | Compare each table and FK against SQL |
| REST endpoint plan | `docs/api-endpoint-plan.md` | Compare route, method, role and DTO with controllers |
| SQL Server schema, constraints and seed records | `docs/RaceDay.sql`, `SQLQuery1_PART1.sql` | Run on a clean SQL Server database and inspect tables/rows |
| Planning artefacts committed before API work | Git history required | Show actual staged commit chronology; cannot be recreated retrospectively |

## Part 2 — REST API
| Requirement | Implementation location | Verification |
|---|---|---|
| ASP.NET Core Web API + EF Core + SQL Server | `src/RaceDay.Api` | `dotnet build` and connect to SQL Server |
| Register/login and hashed passwords | `Controllers/AuthController.cs` | Register, login, inspect that no plaintext password is stored |
| Server-side session and role enforcement | `Auth/`, `SessionAuthorizeAttribute.cs` | Test anonymous, Participant and Organiser requests |
| User profile read/update | `Controllers/UsersController.cs` | GET/PUT `/api/users/me` |
| Event CRUD and public listing | `Controllers/EventsController.cs` | GET/POST/PUT/DELETE `/api/events` |
| Category management | `Controllers/CategoriesController.cs` | GET/POST/PUT/DELETE event categories |
| Enrolments and organiser start lists | `Controllers/EnrolmentsController.cs` | Participant POST `/api/enrolments`; GET own; organiser GET event entries |
| Results | `Controllers/ResultsController.cs` | Organiser POST/PUT; participant GET own |
| Swagger descriptions | `Program.cs`, XML comments | Open `/swagger`; test all endpoints |
| Automated tests | `tests/RaceDay.Api.Tests` | `dotnet test` locally and CI |

## Part 3 — MVC, cloud and containers
| Requirement | Implementation location | Verification |
|---|---|---|
| MVC website consumes the API | `src/RaceDay.Web` and `Services/ApiClient.cs` | Run both projects; verify cookie/session forwarding |
| Public event discovery/details | `Views/Events` | Browse without signing in |
| Participant account and event entry | `AccountController`, `ParticipantController`, dashboard | Register participant, login, enter category, inspect dashboard |
| Organiser event/category/enrolment/result workflow | `EventsController`, `DashboardController`, event views | Register organiser; create event/category; inspect enrolments and record result |
| Purple responsive visual design and new logo | `wwwroot/css/site.css`, `wwwroot/images/raceday-mark.svg` | Test desktop, mobile, keyboard focus and zoom |
| Azure Blob Storage upload | `Controllers/UploadsController.cs` | Configure real Azure connection string and upload test image |
| Docker containers | `src/RaceDay.Api/Dockerfile`, `src/RaceDay.Web/Dockerfile`, `docker-compose.yml` | `docker compose up --build`; verify both URLs |
| GitHub Actions | `.github/workflows/ci.yml` | Push to your own repository and retain green run screenshot |

## Required manual acceptance tests
1. Anonymous user can list and view published events but cannot call protected endpoints.
2. Participant can register and login; password is hashed in SQL.
3. Participant cannot create/update/delete events or manage categories.
4. Organiser can create an event and add categories.
5. Participant can enrol in an event category; duplicate enrolment and full category are rejected.
6. Participant cannot view another participant's entries/results.
7. Organiser can view enrolments only for their own events.
8. Organiser can capture a result; participant can see their own result.
9. A different organiser cannot edit or delete another organiser's event or results.
10. Swagger exposes and documents all API endpoints.
11. Tests pass locally and in GitHub Actions.
12. MVC site works through browser navigation; API cookies persist through the web proxy.
13. Docker Compose starts SQL Server, API and MVC web app.
14. Azure upload works only after a valid storage connection is configured.
15. Keyboard-only use, 200% zoom, contrast, form labels, focus indicators and mobile layout are checked.

## Academic submission items that cannot be truthfully generated on the student's behalf
- The 20 meaningful commits must be made from the student's GitHub account.
- Green CI screenshot must come from the actual repository workflow.
- Video must be recorded by the student with their own voice; AI-generated voices are prohibited in the supplied brief.
- Live SQL/API screenshots must be captured after running the application against the student's configured database.
- Add the student's correct name, student number, GitHub repository URL and unlisted video URL.
