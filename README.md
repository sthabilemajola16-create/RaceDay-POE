RaceDay

PROG6212 – Programming 2B | Portfolio of Evidence (PoE) Part 1: System Planning and Database

RaceDay is a planned full-stack, API-driven event platform for the South African road running, walking and cycling community. Flagship events such as the Comrades Marathon, the Cape Town Cycle Tour and the Soweto Marathon show how large this culture is, yet many smaller races still rely on paper forms, spreadsheets and disconnected messaging (Comrades Marathon Association, n.d.; Cape Town Cycle Tour, n.d.). RaceDay replaces that fragmentation with one place for organisers to manage events and for participants to enter, prepare and track results.

This repository is the single GitHub project for all three PoE parts. Part 1 contains planning artefacts only (ERD, API plan, SQL script, CI that validates /docs). No REST API code is written until Part 2.

1. Brief description of the system

RaceDay will become a containerised, cloud-aware, API-driven sports-technology platform:

Part



Deliverable





Part 1 (this submission)



Relational design (ERD), REST endpoint plan, SQL Server script with seed data, GitHub + CI





Part 2



C# REST API, database connection, unit tests, GitHub Actions build





Part 3



MVC web UI that consumes the API, Azure Blob Storage, Docker

Organisers create and manage events, categories and participant results. Participants browse upcoming events, enter a category, track personal history, and prepare for race day using route information and weather data.





2. User roles

Role-based access control (RBAC) is designed before any API code, and will be enforced at the API in Part 2 and in the MVC UI in Part 3 (Sandhu et al., 1996).

Organiser





Create, edit and delete events  



Manage event categories (distance, fee, age band, capacity)  



Capture and correct participant results  



View all enrolments for events they own



Participant





Create an account and maintain a profile  



Browse published events  



Enter an event by selecting a category  



View own enrolments only  



Track own results / performance history

Public visitors may browse published events, routes, weather snapshots and published result lists without logging in.





3. Answers to the Part 1 questions and learning outcomes



3.1 How is the relational database designed?

Eight entities are modelled (minimum required is six). Primary keys, foreign keys and cardinality are shown below, in [docs/erd.md](docs/erd.md) and in [docs/RaceDay.sql](docs/RaceDay.sql).









Entity



Role in the business





Club



Optional athletics/cycling club for a user





AppUser



Login identity with role Organiser or Participant





RaceEvent



A race or ride owned by one organiser





Category



Distance/fee band belonging to one event





Route



Map/GPX notes for race-day preparation (Blob URL in Part 3)





WeatherSnapshot



Cached weather for race-day preparation





Enrolment



Associative entity: participant enters one category of one event





Result



At most one official result per enrolment

Many-to-many: a participant and an event never share a direct M:N table. They meet through Enrolment (one row per person per event, unique constraint). That is the standard way to store extra facts such as bib number and enrolment status (Connolly and Begg, 2015; W3Schools, n.d.b).

One-to-one: Result.EnrolmentId is unique, so one enrolment produces at most one result.

Table names AppUser and RaceEvent are used in both the ERD and the SQL script because USER and EVENT are reserved in T-SQL. That is a naming choice, not a mismatch (Microsoft, 2024).

3.2 Are there differences between the ERD and the SQL script?

No deliberate differences. Every ERD entity, attribute, PK, FK and relationship is implemented in RaceDay.sql with CREATE TABLE, PRIMARY KEY, FOREIGN KEY / REFERENCES, UNIQUE, NOT NULL and DEFAULT/CHECK constraints (W3Schools, n.d.c; W3Schools, n.d.d; W3Schools, n.d.e).

3.3 How is the REST API planned?

The full six-column table lives in [docs/api-endpoint-plan.md](docs/api-endpoint-plan.md). All routes start with /api/. Groups covered:





Authentication (register, login)  



User profile  



Events  



Categories  



Event enrolments  



Results  



Routes and weather (supports the brief’s race-day preparation features)  



Clubs (registration drop-down)

Each row states HTTP method, route, description, role, JSON body and success plus failure codes (for example 400, 401, 403, 404, 409). Part 2 must follow this plan closely.

REST resources are nouns (/api/events/{id}/enrolments), and verbs stay in the HTTP method (GET, POST, PUT, PATCH, DELETE) (Fielding, 2000; W3Schools, n.d.a).

3.4 How is the SQL script written for SSMS?

[docs/RaceDay.sql](docs/RaceDay.sql) is a single SSMS script that:





Recreates database RaceDay on a clean instance (drops it first if it already exists).



Creates all eight tables with constraints.



Seeds realistic South African data, including at least:





2 organisers (Thabo Mokoena, Ayesha Davids)  



2 participants (Lerato Nkosi, Pieter Botha)  



3 events (Soweto 10 km, Durban half marathon, Stellenbosch cycle classic)  



categories for each event  



sample enrolments  



plus clubs, routes, weather and one captured result so every table has rows

INSERT INTO syntax follows the documented SQL Server / W3Schools pattern of listing columns then values (W3Schools, n.d.f).

3.5 How is role-based design shown before coding?





AppUser.Role is constrained to Organiser or Participant.  



The endpoint plan states who may call each route.  



Organiser-only writes (events, categories, results, full enrolment lists) are never marked public.  



Participants cannot list another athlete’s enrolments; they use /api/enrolments/me and /api/results/me.





4. Repository structure

RaceDay/
├── README.md
├── .gitignore
├── .github/workflows/ci.yml      # Part 1: validates /docs and README
└── docs/
    ├── erd.md                    # ERD (Mermaid + cardinality tables)
    ├── erd.png                   # Export this from erd.md for ARC if a PNG is required
    ├── api-endpoint-plan.md      # Six-column endpoint table
    ├── RaceDay.sql               # SSMS script
    └── ci-green-build.png        # Screenshot of a green GitHub Actions run (add after first pass)





5. Setup notes (Part 1)



5.1 Tools





Git  



SQL Server (Express or Developer) plus SQL Server Management Studio (SSMS)  



A GitHub account (submission is a repository link on ARC; ZIP files are not accepted)



5.2 Clone and inspect

git clone <your-github-repo-url>
cd RaceDay



5.3 Run the database script in SSMS (required for the video)





Open SSMS and connect to (localdb)\MSSQLLocalDB or your instance name.



File → Open → docs/RaceDay.sql.



Execute (F5).



Confirm the batch completes without errors and the final SELECT shows row counts for all eight tables.



Optional check:

USE RaceDay;
SELECT Role, COUNT(*) AS Users FROM dbo.AppUser GROUP BY Role;
SELECT Name, City, Province FROM dbo.RaceEvent;



5.4 Part 2 / Part 3 (not in this submission)

Later: .NET SDK for the API and MVC app, Docker Desktop, and an Azure storage account for GPX/images. Connection strings must stay out of Git (use User Secrets or environment variables).





6. GitHub, commits and CI/CD





One repository is used for Parts 1–3.  



At least 20 meaningful commits per part are required. Commits such as “fixed typo” with no real change do not count. Prefer messages that explain why (for example: “Add Enrolment unique constraint so a participant cannot double-enter”).  



Workflow file: [.github/workflows/ci.yml](.github/workflows/ci.yml). On each push it checks that /docs exists, that the ERD, endpoint plan and SQL script are present, and that the SQL file contains CREATE TABLE, keys and INSERT seed data.



CI/CD green-build screenshot

After the workflow has a green tick on GitHub → Actions, capture the successful run and save it as docs/ci-green-build.png.



If the image does not render yet, run the workflow on GitHub, screenshot the green check, and commit docs/ci-green-build.png.





7. Video presentation

Unlisted YouTube link (replace before ARC submission):

https://www.youtube.com/watch?v=YOUR_UNLISTED_VIDEO_ID

The recording must:





Show the application artefacts (ERD, endpoint plan) and run RaceDay.sql live in SSMS  



Use a real voice-over (no AI voices) explaining ERD decisions, endpoint-plan choices and SQL design  



Remain unlisted and linked from this README





8. Roadmap after Part 1







Part



Technical goal





2



ASP.NET Core Web API, JWT auth, RBAC filters, EF Core or ADO.NET against RaceDay, unit tests, CI build





3



ASP.NET MVC client, same RBAC in the UI, Azure Blob for route/GPX files, Docker compose, live weather client





9. References (Harvard)

Cape Town Cycle Tour (n.d.) Cape Town Cycle Tour. Available at: https://www.capetowncycletour.com/ (Accessed: 25 August 2026).

Comrades Marathon Association (n.d.) Comrades Marathon. Available at: https://www.comrades.com/ (Accessed: 25 August 2026).

Connolly, T. and Begg, C. (2015) Database systems: a practical approach to design, implementation, and management. 6th edn. Harlow: Pearson.

Fielding, R.T. (2000) Architectural styles and the design of network-based software architectures. PhD thesis. University of California, Irvine. Available at: https://www.ics.uci.edu/~fielding/pubs/dissertation/top.htm (Accessed: 25 August 2026).

Microsoft (2024) CREATE TABLE (Transact-SQL). Available at: https://learn.microsoft.com/en-us/sql/t-sql/statements/create-table-transact-sql (Accessed: 25 August 2026).

Sandhu, R.S., Coyne, E.J., Feinstein, H.L. and Youman, C.E. (1996) ‘Role-based access control models’, IEEE Computer, 29(2), pp. 38–47.

W3Schools (n.d.a) HTTP request methods. Available at: https://www.w3schools.com/tags/ref_httpmethods.asp (Accessed: 25 August 2026).

W3Schools (n.d.b) SQL foreign key constraint. Available at: https://www.w3schools.com/sql/sql_foreignkey.asp (Accesse
