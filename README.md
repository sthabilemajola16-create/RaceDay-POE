# RaceDay — PROG6212 POE

RaceDay is a small South African road-event system. Clubs put on runs, walks and cycle sportives. People enrol online instead of filling in a paper form on the morning. This repository holds Part 1 (database + API plan) and Part 2 (the ASP.NET Core Web API, session auth, Swagger, xUnit tests and GitHub Actions).


---

## What I built

Part 1 gave me a real schema. Part 2 is that schema, running.

| Artefact | Where it is |
|---|---|
| Entity relationship diagram | `docs/erd.md`, `docs/erd.png`, `ST10486077_ERD.drawio` |
| SQL Server script (create, keys, checks, seed) | `docs/RaceDay.sql` |
| REST endpoint plan (the spec I coded against) | `docs/api-endpoint-plan.md` |
| ASP.NET Core Web API | `src/RaceDay.Api/` |
| xUnit tests (auth, events, roles, enrolments, results) | `tests/RaceDay.Api.Tests/` |
| GitHub Actions (docs checks + `dotnet test`) | `.github/workflows/ci.yml` |

The eight table names in the ERD, the SQL script and Entity Framework are the same: `AppUser`, `Category`, `Club`, `Enrolment`, `RaceEvent`, `Result`, `Route`, `WeatherSnapshot`. I did not invent a second model for C#.

---

## The two roles

There is no admin super-user. SQL and EF only allow `Organiser` or `Participant`. The role is chosen at registration and stored on `AppUser`. After login a **server-side session** holds the user ID and that role. The browser (and Swagger) just sends the HTTP-only cookie `.RaceDay.Session`.

### Organiser

This is the person who owns a race weekend. They can:

- create, update and delete **their own** events (name, description, date, venue / city / province, run–walk–cycle type)
- add age or distance **categories** (Under 20, Senior, 10 km, 21.1 km, and so on)
- see every **enrolment** on those events
- type in **finish times and positions** after the chute
- keep route notes and a weather snapshot for race-day prep
- look up a participant’s public profile when they are capturing results

They cannot enrol themselves through `POST /api/enrolments`. If they try, the API returns **403 Forbidden**.

### Participant

This is the runner, walker or cyclist. They can:

- browse published events and categories
- **enrol** by picking one category on an event (the row stores participant + event + category, and a bib number)
- see **their own** enrolments and **their own** results
- update their profile (name, phone, club, date of birth — not their role)

They cannot create events, edit categories, or capture results. Those calls also return **403**.

### What stays public

Register, login, the published event list, categories on an event, published results, route notes, weather, and the club drop-down. Everything else needs a session cookie. No password is stored in plain text — only `PasswordHash`.

A longer “who can do what” table sits in `docs/api-endpoint-plan.md`.

---

## Folder structure

```
POE PROG6212/
├── README.md
├── RaceDay.sln
├── SQLQuery1_PART1.sql
├── docs/
│   ├── erd.md
│   ├── RaceDay.sql
│   ├── api-endpoint-plan.md
│   └── ci-green-build.png          ← 
├── src/RaceDay.Api/                ← Web API, EF Core, session auth, Swagger
├── tests/RaceDay.Api.Tests/        ← xUnit, in-memory database so CI does not need SQL Server
└── .github/workflows/ci.yml
```

---

## What you need on the machine

- Windows 10/11
- [SQL Server](https://www.microsoft.com/sql-server) (LocalDB, Express or Developer) and **SQL Server Management Studio**
- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- Visual Studio 2022 (ASP.NET and web development workload) *or* `dotnet` from a terminal
- Git, and a GitHub account that can push this repo

---

## Setup and run — database

1. Clone the repository.
2. Open **SQL Server Management Studio** and connect (on many student laptops that is `(localdb)\MSSQLLocalDB`).
3. Open `docs/RaceDay.sql` and press **F5**.

The script rebuilds `RaceDay` with the eight tables, keys, check constraints and sample South African data (KZN Athletics Club, Soweto Sunrise 10 km, Durban Beachfront Half, Stellenbosch Winelands Classic).

The API connection string in `src/RaceDay.Api/appsettings.json` already points at LocalDB:

```text
Server=(localdb)\MSSQLLocalDB;Database=RaceDay;Trusted_Connection=True;MultipleActiveResultSets=true;TrustServerCertificate=True
```

If the database is empty when the API starts, EF `EnsureCreated` plus seed data will fill the same clubs, users and events so Swagger still has something to show. Demo passwords are re-hashed to `Password1!` on startup.

---

## Setup and run — API

1. Open `RaceDay.sln` in Visual Studio 2022, **or** from the repo root run:

```text
dotnet restore RaceDay.sln
dotnet run --project src/RaceDay.Api/RaceDay.Api.csproj
```

2. The profile launches Swagger (`launchUrl` is `swagger`). In a browser that is usually:

- https://localhost:7188/swagger
- http://localhost:5188/swagger

3. You do not need Postman. Every route is listed with a description, the body it expects, and the status codes it returns.

### Demo logins (password for all of them is `Password1!`)

| Email | Role |
|---|---|
| `thabo.organiser@raceday.co.za` | Organiser |
| `ayesha.organiser@raceday.co.za` | Organiser |
| `lerato.runner@email.com` | Participant |
| `pieter.cyclist@email.com` | Participant |

How I try it in Swagger:

1. `POST /api/auth/login` as Thabo. Then `GET /api/auth/session` — you should see his user ID and `Organiser`.
2. `POST /api/events` to put a new race on the calendar. Add a category. Stay on that tab so the cookie is still there.
3. Log out. Log in as Lerato. `POST /api/enrolments` with that event and category. `GET /api/enrolments/me` should show the bib.
4. Log in as Thabo again and `POST /api/results` with her enrolment id. Lerato can then `GET /api/results/me`.
5. Swap the roles on purpose. A participant posting an event, or an organiser posting an enrolment, should come back **403**. A call with no cookie should come back **401**.

---

## Unit tests

From the repo root:

```text
dotnet test RaceDay.sln
```

The test project uses `WebApplicationFactory` and EF InMemory so GitHub Actions does not need SQL Server. The tests cover both the happy path and the failure I would expect:

- register (organiser + participant, hashed password, duplicate email **409**, unknown role **400**)
- login / session / logout (wrong password **401**, no cookie **401**)
- organiser event CRUD, drafts staying off the public list
- the other role being rejected (**403**) on events, categories, enrolments and results
- a participant enrolment actually writing participant + event + category, plus a second entry on the same event (**409**)
- organiser capturing a finish time; participant reading their own result

Those tests must pass on the laptop **and** on GitHub Actions.

---

## GitHub and CI/CD

The workflow is `.github/workflows/ci.yml`. On every push it:

- checks the required folders and files are present
- greps this README for the two roles, the video link, CI/CD, W3Schools and References
- checks the SQL script and the endpoint plan
- restores, builds and runs `dotnet test`

### Green-build screenshot

After a push shows a green tick on the Actions tab, save that screenshot as `docs/ci-green-build.png` (see `docs/ci-screenshot-instructions.txt`). Do not paste a mocked image.

![GitHub Actions CI/CD — successful green build](doc  s/ci-green-build.png)

If the picture is missing, the run has not been captured yet. The workflow file is in the repo and will run on the next push.

---

## How I work on GitHub for this part

- Push from **my own** GitHub account.
- Part 2 lives in this repo (branch such as `part-2-api` is fine).
- Aim for **20 or more meaningful commits** (project file, entities, auth/session, each resource, Swagger text, tests, workflow, README screenshot).
- Do not commit `bin/`, `obj/`, secrets, or a live connection string with a password.

---

## Demo video

 In the recording I will:

1. show the repo layout and this README
2. run the SQL script (or show the existing `RaceDay` database)
3. walk the endpoint plan next to Swagger
4. register and log in as Organiser and as Participant, and show the session holding user ID and role
5. create an event, add a category, enrol,capture a result, and show the **403**s when the wrong role is used
6. run the unit tests locally and open the green GitHub Actions run

---

## References

Fielding, R.T. (2000) *Architectural styles and the design of network-based software architectures*. Doctoral dissertation. University of California, Irvine.

Microsoft (n.d.) *ASP.NET Core fundamentals*. Available at: https://learn.microsoft.com/aspnet/core (Accessed: 15 October 2026).

Sandhu, R.S., Coyne, E.J., Feinstein, H.L. and Youman, C.E. (1996) ‘Role-based access control models’, *IEEE Computer*, 29(2), pp. 38–47.

W3Schools (n.d.a) *HTTP status messages*. Available at: https://www.w3schools.com/tags/ref_httpmessages.asp (Accessed: 14 October 2026).

W3Schools (n.d.b) *SQL primary key*. Available at: https://www.w3schools.com/sql/sql_primarykey.asp (Accessed: 14 October 2026).

