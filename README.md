RaceDay is a small South African road-event system. Clubs put on runs, walks and cycle sportives. People enrol online instead of filling in a paper form on the morning. This repository is the full POE: Part 1 is the database and API plan, Part 2 is the ASP.NET Core Web API, and Part 3 is the ASP.NET Core MVC front end that talks only to that API, plus Azure Blob Storage (through the API) and Docker.

Unlisted demo video (replace this link before ARC handover):
https://youtu.be/REPLACE_WITH_YOUR_UNLISTED_VIDEO



Docker — start the MVC app with one command

The marker should be able to run the containerised MVC site without opening Visual Studio. From the repository root (POE PROG6212/):

docker build -t raceday-web -f src/RaceDay.Web/Dockerfile .
docker run --rm -p 8080:8080 -e Api__BaseUrl=http://host.docker.internal:5188 raceday-web

Then open http://localhost:8080. That is the same RaceDay UI you get with dotnet run; the container does not talk to SQL Server or Azure on its own. It only calls the Part 2 API. Start the API on the host first (http://localhost:5188) so enrolments, banners and profile photos still land in the same database and blob container.

If you prefer a single compose command once both Dockerfiles are in the tree:

docker compose up --build

docker compose is optional. The two-line docker build / docker run pair above is the command I demonstrate in the video.



What lives in this repository







Part



What it is



Where it is





1



ERD, SQL Server script, REST plan



docs/





2



ASP.NET Core Web API, session cookie auth, xUnit



src/RaceDay.Api, tests/RaceDay.Api.Tests





3



ASP.NET Core MVC (no database of its own), Blob uploads via the API, Docker



src/RaceDay.Web, src/RaceDay.Web/Dockerfile

The MVC project must not contain a DbContext, connection strings, or copied business rules. If a participant enrols, the MVC controller posts to /api/enrolments. If an organiser uploads a banner, the MVC form posts the file to the API; the API puts the bytes in Azure Blob Storage and saves the URL. The MVC app only ever displays that URL.



Folder structure

POE PROG6212/
├── README.md                          ← you are here (Docker run, video, CI screenshot)
├── RaceDay.sln
├── SQLQuery1_PART1.sql
├── ERD_ST10486077.png
├── ST10486077_ERD.drawio
├── docs/
│   ├── erd.md
│   ├── erd.png
│   ├── RaceDay.sql
│   ├── api-endpoint-plan.md
│   ├── ci-screenshot-instructions.txt
│   └── ci-green-build.png             ← green GitHub Actions run
├── src/
│   ├── RaceDay.Api/                   ← Part 2 Web API (SQL + session + Blob)
│   └── RaceDay.Web/                   ← Part 3 MVC (HTTP client only)
│       ├── Dockerfile                 ← multi-stage ASP.NET Core image
│       ├── Controllers/
│       ├── Views/
│       ├── wwwroot/css/
│       └── Services/                  ← typed client for the Part 2 API
├── tests/
│   └── RaceDay.Api.Tests/
└── .github/workflows/ci.yml           ← structure checks + build API + build MVC



The two roles (same story from Part 1 through Part 3)

There is no admin super-user. An account is Organiser or Participant. SQL, the API and the MVC menus all use those two strings.

Organiser

This is the person who owns a race weekend. After they register and log in, a server-side session stores their user ID and the role Organiser. In the MVC site they get their own nav (Dashboard, Events, Categories, Enrolments, Results) and they can:





see a dashboard: events they created, how many people enrolled on each one, and the next start dates



create, edit and delete events they own — name, description, date, location, distance and event type (run / walk / cycle)



upload an event banner when they create or edit (the file goes MVC → API → Azure Blob; the event detail page shows the image)



add age or distance categories to an event (Under 20, Senior, 10 km, 21.1 km, and so on)



open an enrolments page for one event and see every participant, the category they picked, and enrolment status



capture finish times and finishing positions after the event, then publish them so participants can see “47th of 312 finishers”

They cannot enrol themselves on the participant enrolment route. The API still returns 403 Forbidden if they try.

Participant

This is the runner, walker or cyclist. Their session stores user ID and the role Participant. Their nav is Home, Events, My events, My results, Profile. They can:





browse upcoming events with filters (sport type, province, date)



open a detail page with full information, categories, and the banner image from Blob Storage



enrol on an event and pick a category; the enrolment is written immediately through the API



see My events with colour-coded status (green for Confirmed, amber for Pending)



open My results across completed races: event name, date, category, finish time, position



update profile details and upload a profile picture (again MVC → API → Blob, never Azure from the browser)

They cannot create events, edit categories, or capture results. Those MVC pages are hidden in their menu, and the API still returns 403.

Unauthenticated visitors

Anyone can register, log in, and look at the public event list. Protected organiser and participant pages redirect to login. Logout clears the session cookie and sends them back to the public home page.



Authentication and session (MVC talks to the API)

Register and login forms live in the MVC app. They do not hash passwords or write to SQL. They POST to /api/auth/register and /api/auth/login. The API stores user ID and role in the .RaceDay.Session cookie. The MVC app keeps a matching session so the layout can show the right menu on every page.







What the rubric asked for



How it works





Register and login through the MVC app



AccountController calls the Part 2 auth endpoints





Session holds user ID and role on every page



MVC session + API session cookie





Distinct interface and navigation per role



_Layout.cshtml swaps organiser vs participant menus





Unauthenticated users cannot open protected pages



[Authorize] / role filters redirect to login





Logout ends the session and redirects



POST /api/auth/logout, then MVC session cleared

Demo accounts (password for all of them is Password1!):







Email



Role





thabo.organiser@raceday.co.za



Organiser





ayesha.organiser@raceday.co.za



Organiser





lerato.runner@email.com



Participant





pieter.cyclist@email.com



Participant



Organiser pages (Part 3)

These are Razor views. Every load and post goes through the API client — there is no RaceDayContext in the MVC project.





Dashboard — count of my events, enrolments per event, upcoming dates.



Events — list, create, edit, delete. Fields: name, description, date, location, distance, event type. Banner file input on create and edit.



Categories — add age or distance categories to a chosen event.



Enrolments — participants on one event, category chosen, status badge.



Results — finish time and position for each enrolment after the race, then publish.

Invalid dates, missing names or a banner that is not an image come back as friendly validation messages on the form. The site does not dump a yellow ASP.NET error page on the user.



Participant pages (Part 3)





Home — upcoming events, filter chips for run / walk / cycle and province.



Event detail — copy, date, venue, categories, banner from Blob, Enter event for a logged-in participant.



Enrol — pick a category; the API records it immediately.



My events — everything I entered, with Confirmed / Pending colour coding.



My results — race history: name, date, category, time, “47th of 312 finishers”.



Profile — name, phone, club, date of birth, profile picture upload.



Azure Blob Storage (API only)

The brief is strict here: the MVC application must not talk to Azure. I keep the storage account name, key and container on the API (AzureBlob settings / user secrets), never in the MVC appsettings.

Flow for an event banner (profile pictures use the same pattern):





Organiser chooses a file on the create/edit event form.



MVC posts multipart/form-data to the API (POST /api/events/{id}/banner).



The API validates the content type, uploads to the Blob container, and stores the public URL on the event row.



Event detail in MVC binds that URL to an <img> tag. If Azure is not configured on a student laptop, the API returns a clear message and the form stays up; the rest of the site still works.

That is the “greatly exceeds” path: file sent from MVC to API, bytes in Azure, URL displayed in MVC.



UI — RaceDay brand

The look is sporting and a bit loud on purpose: dark navy, race-tape yellow, and white space, with a condensed heading font and Bootstrap 5 for layout so the same pages work on a phone in the start village. Status pills are colour-coded (green Confirmed, amber Pending, grey Cancelled). Banners and profile photos give the pages actual race imagery instead of empty cards. Nav, buttons, tables and forms share one stylesheet so an organiser dashboard and a participant profile still feel like the same product.



What I built in Part 1

The brief asked for a real schema, not a sketch. I modelled eight tables so I already have room for clubs, route notes and a weather snapshot, not only users and events.







Artefact



Where it is





Entity relationship diagram (Crow’s foot, written up)



docs/erd.md





ERD image for ARC



docs/erd.png (also ERD_ST10486077.png at the repo root)





SQL Server script — create database, keys, checks, seed data



docs/RaceDay.sql





REST endpoint plan (/api/…, roles, bodies, responses)



docs/api-endpoint-plan.md





GitHub Actions workflow



.github/workflows/ci.yml

docs/erd.md and docs/RaceDay.sql use the same eight names, keys and relationships.



What I built in Part 2

Part 2 is the ASP.NET Core 8 Web API in src/RaceDay.Api. Entity Framework Core maps the Part 1 tables. Auth is a session cookie, not a JWT printed in Swagger. Passwords go through PasswordHasher<AppUser>. Swagger is on by default so a marker can try every route. xUnit tests in tests/RaceDay.Api.Tests cover register/login, organiser event changes, 403 for the wrong role, and an enrolment that actually writes participant + event + category.

Public without a session: register, login, published events, categories, published results, routes, weather, clubs. Everything else needs the cookie.



What you need on the machine





Windows 10/11



SQL Server (LocalDB, Express or Developer) and SQL Server Management Studio



.NET 8 SDK



Visual Studio 2022 (ASP.NET and web development workload) or the dotnet CLI



Docker Desktop for the containerised MVC app



An Azure Storage account (or the Azurite emulator) so banner and profile uploads can leave the API



Git, and a GitHub account that can push this repo



Setup and run — database (Part 1)





Clone the repository and open the folder.



Open SQL Server Management Studio and connect to your local instance (for many student laptops that is (localdb)\MSSQLLocalDB or localhost).



Open docs/RaceDay.sql (or SQLQuery1_PART1.sql — they are the same schema).



Press F5 / Execute.

The script drops RaceDay if it is already there, creates it again, builds the eight tables, then inserts sample South African data (KZN Athletics Club, Soweto Sunrise 10 km, Durban Beachfront Half, Stellenbosch Winelands Classic). The last SELECT prints row counts so you can see the seed landed.

If SSMS complains that the database is in use, close other query windows connected to RaceDay and run the script again.

Quick check that the schema matches the ERD

USE RaceDay;
SELECT TABLE_NAME FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_TYPE = 'BASE TABLE' ORDER BY TABLE_NAME;

You should see AppUser, Category, Club, Enrolment, RaceEvent, Result, Route, WeatherSnapshot.



Setup and run — API (Part 2)





Open RaceDay.sln in Visual Studio 2022, or from the repo root run the commands below.



Check src/RaceDay.Api/appsettings.json for the RaceDay connection string (same instance as SSMS).



Put Blob settings in user secrets or environment variables — not in Git:

 AzureBlob__ConnectionString=DefaultEndpointsProtocol=https;AccountName=...;AccountKey=...;EndpointSuffix=core.windows.net
 AzureBlob__Container=raceday-images



Start the API:

 dotnet run --project src/RaceDay.Api



Open Swagger at https://localhost:7188/swagger (HTTP is http://localhost:5188/swagger).



Call POST /api/auth/register then POST /api/auth/login. Stay on that tab so the session cookie is sent. Try an Organiser route and a Participant route and confirm 403 when the role is wrong.

Every route, with body and expected status codes, is in docs/api-endpoint-plan.md. If it is not visible and tryable in Swagger, it is not done.

Unit tests

From the repository root:

dotnet test

Those tests must pass on the laptop and on GitHub Actions.



Setup and run — MVC (Part 3)

The MVC app is a client. Start the API first, then:

dotnet run --project src/RaceDay.Web

appsettings.json in the MVC project only needs the API base URL (for example http://localhost:5188). It does not get a SQL connection string or an Azure key. Browse to the MVC URL, register or use a demo login, and you should see the role-specific menu straight away.

To prove there is no leaked database access, search the MVC project for UseSqlServer / DbContext — those belong only under src/RaceDay.Api.



Dockerfile notes (best practice, not a one-liner dump)

src/RaceDay.Web/Dockerfile is a multi-stage file:





sdk image restores and publishes the MVC project.



aspnet runtime image copies the published output only — no SDK, no source, no secrets.



The app listens on port 8080 (ASPNETCORE_URLS=http://+:8080) so docker run -p 8080:8080 maps cleanly.



Api__BaseUrl is injected at run time so the same image works on a laptop (host.docker.internal) and in CI.

The containerised site must behave like the local dotnet run site: same pages, same session rules, same validation messages. If a banner upload works locally through the API, it must still work when MVC is inside Docker, because Docker never talks to Blob itself.



GitHub and CI/CD

The workflow is .github/workflows/ci.yml. On every push (including the Part 3 branch) GitHub Actions:





checks the required folders and files (docs/, README, SQL, ERD, endpoint plan, src/RaceDay.Api, src/RaceDay.Web, Dockerfile, the workflow itself)



greps this README for the two roles, the video link, CI/CD, Docker, W3Schools and References



checks the SQL script still creates a database, tables, keys, seed rows, and both roles



checks the endpoint plan still has HTTP method, /api/, role, body, response, register, login, enrolment and results



runs dotnet restore, dotnet build and dotnet test on the API



runs dotnet build on the MVC project so a broken view or missing package turns the build red

Green-build screenshot

After a push shows a green tick on the Actions tab, save that screenshot as docs/ci-green-build.png (see docs/ci-screenshot-instructions.txt). Do not paste a mocked image.



If the picture is missing, the run has not been captured yet. The workflow file is still in the repo and will run on the next push.



How I work on GitHub for Part 3





Push from my own GitHub account.



Use a clearly named branch or folder for Part 3 (for example part-3-mvc).



Aim for 20 or more meaningful commits for this part (MVC project, layout/theme, auth/session, organiser pages, participant pages, Blob upload on the API, Dockerfile, workflow, README Docker command, screenshot).



Do not commit bin/, obj/, .env, storage keys, or a live connection string with a password.

ARC submission is the GitHub link. The repo must contain source, Dockerfile, this README (with the Docker run command) and the YouTube link.



Demo video (unlisted YouTube)

The ARC submission needs an unlisted YouTube video. In the recording I will:





show the repo layout and this README (folder structure, Docker command, video link)



run the API and the MVC site locally



register and log in as Organiser and as Participant, show the session holding user ID and role, and the two different menus



walk the organiser dashboard, event CRUD with banner upload, categories, enrolments and results entry



walk the participant home, event detail, enrol, my events, my results (time and position) and profile picture



show that a Blob URL comes back from the API and renders on the event page — MVC has no Azure SDK



build and run the MVC Docker image with the command at the top of this file, and click through the same pages



open the green GitHub Actions run (API + MVC build)

Paste the real unlisted URL at the top of this file before handing in.



References

Fielding, R.T. (2000) Architectural styles and the design of network-based software architectures. Doctoral dissertation. University of California, Irvine.

Microsoft (n.d.a) ASP.NET Core fundamentals. Available at: https://learn.microsoft.com/aspnet/core (Accessed: 15 October 2026).

Microsoft (n.d.b) Containerize a .NET app. Available at: https://learn.microsoft.com/dotnet/core/docker/build-container (Accessed: 28 October 2026).

Microsoft (n.d.c) Azure Blob Storage. Available at: https://learn.microsoft.com/azure/storage/blobs (Accessed: 28 October 2026).

Sandhu, R.S., Coyne, E.J., Feinstein, H.L. and Youman, C.E. (1996) ‘Role-based access control models’, IEEE Computer, 29(2), pp. 38–47.

W3Schools (n.d.a) HTTP status messages. Available at: https://www.w3schools.com/tags/ref_httpmessages.asp (Accessed: 14 October 2026).

W3Schools (n.d.b) SQL primary key. Available at: https://www.w3schools.com/sql/sql_primarykey.asp (Accessed: 14 October 2026).

W3Schools (n.d.c) HTML file upload. Available at: https://www.w3schools.com/tags/att_input_type_file.asp (Accessed: 28 October 2026).
