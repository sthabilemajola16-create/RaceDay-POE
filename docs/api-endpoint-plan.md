# RaceDay API endpoint plan

**Student:** Andiswa Sthabile Majola (ST10486077)  
**Module:** PROG6212 – Programming 2B  
**System:** RaceDay  
**Purpose:** This is the specification I will build in Part 2. Every route still starts with `/api/` as required. The tables below are written so Swagger can show the purpose, the body it expects, and the response it returns, without extra tooling.

Part 2 is not a new design. It is this plan, wired to the Part 1 ERD and `RaceDay.sql` through Entity Framework Core (code first), with a **server-side session** that remembers who is logged in and which role they have.

---

## How I read the brief (in plain language)

RaceDay is a community race system. An **Organiser** puts events on the calendar, sets age or distance categories, watches who entered, and later types in finish times. A **Participant** looks at events, picks a category, enrols, and later checks their own result. Nobody should see a raw password, and a participant should not be able to edit someone else’s event just because they found the URL.

The question paper is very specific about **session management**, not a token I would print in Swagger and forget about. On login I will create a server-side session that stores the **user ID** and **role**. Later requests send the session cookie. If the cookie is missing I return **401**. If the person is logged in but the role is wrong I return **403**. That is what the MVC client in Part 3 will need as well.

Passwords go into `AppUser.PasswordHash` only. I will hash them with ASP.NET Identity’s hasher (or BCrypt) and never send the hash back in a response.

---

## Roles used in this plan

| Value in the table | What it means in Part 2 |
|---|---|
| None (public) | No login required. Register, login, and a small amount of browsing stay open so Swagger and later the website can load a home page. |
| Any (logged in) | A valid session exists. Organiser or Participant. Used for “my profile”. |
| Organiser | Session exists and `Role` is `Organiser`. |
| Participant | Session exists and `Role` is `Participant`. |

`AppUser.Role` is constrained in SQL to `Organiser` or `Participant`. I will not invent a third role.

**Event type** on the brief is run, walk or cycle. That matches `RaceEvent.SportType`: `Running`, `Walking`, `Cycling`. **Distance** lives on `Category` (5 km, 10 km, 21.1 km, Under 20, Senior, and so on) because one Soweto morning can offer more than one distance. Location is stored as venue, city and province so the South African events in the seed data stay realistic.

---

## Authentication and session

Register is public on purpose: you cannot log in until you have an account. Login creates the session. Logout clears it. `/api/auth/session` is there so I can prove in Swagger that the server actually stored the user ID and role — that is the “greatly exceeds” line on the rubric, not a guess.

| HTTP Method | Route | Description | Role Required | Request Body (if any) | Expected Response |
|---|---|---|---|---|---|
| POST | `/api/auth/register` | Opens a RaceDay account. The person sends their details and chooses Organiser or Participant. The password is hashed before it is saved. Role cannot be changed later through the profile screen. | None (public) | `{ email, password, firstName, lastName, role, phone, clubId, dateOfBirth }` | **201 Created** profile only (no password, no hash). **400 Bad Request** if a field is missing, the email looks wrong, the password is too short, or the role is not Organiser/Participant. **409 Conflict** if that email is already registered. |
| POST | `/api/auth/login` | Checks email and password. On success the server writes a session that holds `userId` and `role`, and sets an HTTP-only cookie. | None (public) | `{ email, password }` | **200 OK** `{ userId, email, firstName, lastName, role }`. **400 Bad Request** if email or password is blank. **401 Unauthorized** if the email is unknown or the password does not match the hash. |
| POST | `/api/auth/logout` | Ends the session so the cookie can no longer call protected routes. | Any (logged in) | None | **204 No Content**. **401 Unauthorized** if there is no session. |
| GET | `/api/auth/session` | Returns the current session so I can show in Swagger that user ID and role sit on the server, not in a JWT I copied into a header. | Any (logged in) | None | **200 OK** `{ userId, role, email }`. **401 Unauthorized** if the cookie is missing or the session expired. |

Non-public routes use the same cookie. Swagger will send it automatically once login has been called in that browser tab.

---

## User profile

Both roles need to see and tidy their own details. An organiser looking up a participant (for bib numbers or results) is allowed; a participant looking up another person is not.

| HTTP Method | Route | Description | Role Required | Request Body (if any) | Expected Response |
|---|---|---|---|---|---|
| GET | `/api/users/me` | Shows the logged-in person’s own profile. | Any (logged in) | None | **200 OK** profile (email, names, phone, club, date of birth, role). **401 Unauthorized** if there is no session. |
| PUT | `/api/users/me` | Updates name, phone, club and date of birth. Role and email stay as they were at registration so people cannot promote themselves. | Any (logged in) | `{ firstName, lastName, phone, clubId, dateOfBirth }` | **200 OK** updated profile. **400 Bad Request** if validation fails. **401 Unauthorized**. **404 Not Found** if the user row is gone. |
| GET | `/api/users/{id}` | Organiser reads a participant’s public fields when capturing results or checking an entry list. | Organiser | None | **200 OK** public fields only. **401 Unauthorized**. **403 Forbidden** if a Participant calls this. **404 Not Found**. |

---

## Events

Organisers create, change and remove events they own. Both roles (and the public browse list) can **view** events. Each event stores a name, description, date window, location (venue / city / province) and event type. Distance is attached through categories.

| HTTP Method | Route | Description | Role Required | Request Body (if any) | Expected Response |
|---|---|---|---|---|---|
| GET | `/api/events` | Lists published events so people can browse what is coming up. | None (public) | Query: `sportType`, `province`, `from`, `to` | **200 OK** array (empty is still 200). **400 Bad Request** if a filter value is not valid. |
| GET | `/api/events/{id}` | One event with its categories, a short route summary, and the latest weather snapshot if we have one. | None (public) | None | **200 OK** event detail. **404 Not Found**. |
| POST | `/api/events` | Organiser creates an event. `OrganiserId` is taken from the session, not from the body, so you cannot create an event in someone else’s name. | Organiser | `{ name, description, sportType, venue, city, province, startDateTime, endDateTime, status, routeSummary }` | **201 Created**. **400 Bad Request** (dates the wrong way around, unknown sport type, blank name). **401 Unauthorized**. **403 Forbidden** if a Participant posts here. |
| PUT | `/api/events/{id}` | Organiser updates an event they own. | Organiser | Same fields as create | **200 OK**. **400 Bad Request**. **401/403** (wrong role, or not the owner). **404 Not Found**. |
| DELETE | `/api/events/{id}` | Organiser deletes an event they own, but only if confirmed enrolments are not sitting on it. | Organiser | None | **204 No Content**. **401/403**. **404 Not Found**. **409 Conflict** if people are already entered. |

`sportType` must be `Running`, `Walking` or `Cycling`. Status stays in line with the SQL check: `Draft`, `Published`, `Closed`, `Completed`.

---

## Categories

The brief’s examples are Under 20, Senior, 10 km, 21 km. Those are rows on `Category`, not extra event types. Organisers define them per event. Both roles can read them when they look at an event.

| HTTP Method | Route | Description | Role Required | Request Body (if any) | Expected Response |
|---|---|---|---|---|---|
| GET | `/api/events/{eventId}/categories` | Lists the age or distance categories for one event (name, km, fee, age band, cap). | None (public) | None | **200 OK** array. **404 Not Found** if the event does not exist. |
| POST | `/api/events/{eventId}/categories` | Organiser adds a category to an event they own. | Organiser | `{ name, distanceKm, entryFeeZar, maxParticipants, minAge, maxAge }` | **201 Created**. **400 Bad Request** (distance ≤ 0, max age below min age). **401/403**. **404** event missing. |
| PUT | `/api/categories/{id}` | Organiser updates a category on an event they own. | Organiser | Same fields as create | **200 OK**. **400**. **401/403**. **404**. |
| DELETE | `/api/categories/{id}` | Organiser removes a category that nobody has enrolled into yet. | Organiser | None | **204 No Content**. **401/403**. **404**. **409 Conflict** if enrolments exist. |

---

## Event enrolments

This is the link the brief asks for: Participant + event + selected category. The unique constraint `(EventId, ParticipantId)` in SQL stops a person entering the same event twice.

| HTTP Method | Route | Description | Role Required | Request Body (if any) | Expected Response |
|---|---|---|---|---|---|
| POST | `/api/enrolments` | Participant enters an event by choosing a category. The session supplies `participantId`. A bib number is assigned when the entry is confirmed. | Participant | `{ eventId, categoryId }` | **201 Created** enrolment (event, category, bib, status). **400 Bad Request** if that category does not belong to that event. **401 Unauthorized**. **403 Forbidden** if an Organiser tries to enrol as a runner. **404 Not Found**. **409 Conflict** if they are already entered. |
| GET | `/api/enrolments/me` | Participant lists their own entries and the events attached to them. | Participant | None | **200 OK** array. **401/403**. |
| GET | `/api/events/{id}/enrolments` | Organiser sees every enrolment on an event they own. | Organiser | None | **200 OK** array. **401/403** (including “you do not own this event”). **404**. |
| PATCH | `/api/enrolments/{id}` | Participant may withdraw. Organiser may update bib number or status (DNS, DNF, and so on). | Participant or Organiser | `{ status }` or `{ bibNumber, status }` | **200 OK**. **400**. **401/403** if it is not your enrolment / not your event. **404**. **409** if the status change is not allowed. |

---

## Results

After the finish chute, the organiser types finish time and overall position against an enrolment. Participants only see **their** results. A published list on the event is public so family can look up a name later.

| HTTP Method | Route | Description | Role Required | Request Body (if any) | Expected Response |
|---|---|---|---|---|---|
| POST | `/api/results` | Organiser captures a result for one enrolment on an event they own. `CapturedByOrganiserId` comes from the session. | Organiser | `{ enrolmentId, finishTime, positionOverall, resultStatus }` | **201 Created**. **400 Bad Request**. **401/403**. **404**. **409 Conflict** if that enrolment already has a result (SQL unique on `EnrolmentId`). |
| PUT | `/api/results/{id}` | Organiser corrects a result they captured (wrong gun time, wrong place). | Organiser | `{ finishTime, positionOverall, resultStatus }` | **200 OK**. **400**. **401/403**. **404**. |
| GET | `/api/results/me` | Participant reads their own result history. | Participant | None | **200 OK** array. **401/403** if an Organiser hits this route. |
| GET | `/api/events/{id}/results` | Published results for an event. | None (public) | None | **200 OK** array. **404** if the event is missing. |

`resultStatus` follows the SQL check: `Finished`, `DNF`, `DSQ`, `DNS`.

---

## Route information (race-day preparation)

These extra routes are already in the Part 1 ERD. They are not required by the minimum bullet list, but they keep the API honest with the database I already submitted.

| HTTP Method | Route | Description | Role Required | Request Body (if any) | Expected Response |
|---|---|---|---|---|---|
| GET | `/api/events/{id}/routes` | Route notes and GPX / blob URLs for an event. | None (public) | None | **200 OK** array. **404**. |
| POST | `/api/events/{id}/routes` | Organiser adds a route record. Part 3 can upload the actual GPX file to Azure Blob and store the URL here. | Organiser | `{ name, distanceKm, elevationGainM, mapNotes, gpxUrl }` | **201 Created**. **400**. **401/403**. **404**. |

---

## Live weather (race-day preparation)

| HTTP Method | Route | Description | Role Required | Request Body (if any) | Expected Response |
|---|---|---|---|---|---|
| GET | `/api/events/{id}/weather` | Latest stored weather snapshot. Part 3 may refresh this from a live weather API. | None (public) | None | **200 OK** snapshot. **404** if the event or snapshot is missing. |
| POST | `/api/events/{id}/weather` | Organiser stores a snapshot (temperature, condition, wind, humidity, source). | Organiser | `{ temperatureC, condition, windKph, humidityPercent, source }` | **201 Created**. **400**. **401/403**. **404**. |

---

## Clubs (supporting profile data)

| HTTP Method | Route | Description | Role Required | Request Body (if any) | Expected Response |
|---|---|---|---|---|---|
| GET | `/api/clubs` | Club list for the registration drop-down (KZN Athletics, Cape Town Road Cycling, and so on). | None (public) | None | **200 OK** array. |

---

## Who can do what

| Capability | Organiser | Participant | Public |
|---|---|---|---|
| Register / login | Yes | Yes | Register and login only |
| See current session (user ID + role) | Yes | Yes | No |
| Browse events, categories, routes, weather, published results | Yes | Yes | Yes |
| Create / update / delete events and categories | Yes (own events) | No (**403**) | No (**401**) |
| Capture and edit results | Yes (own events) | No (**403**) | No (**401**) |
| View all enrolments for owned events | Yes | No (**403**) | No (**401**) |
| Enter an event (choose a category) | No (**403**) | Yes | No (**401**) |
| View own enrolments and own results | No (**403**) | Yes | No (**401**) |
| Update own profile | Yes | Yes | No (**401**) |

I am not hiding forbidden routes behind a fake **404**. The MVC front end needs a clean **401** vs **403** so it can say “please log in” or “this is an organiser tool” (Sandhu et al., 1996). HTTP codes follow the usual REST meaning (Fielding, 2000; W3Schools, n.d.a).

---

## Swagger (how each endpoint will look in the browser)

Part 2 asks for Swagger so a marker can open the API in a browser and try every route without Postman. I will:

1. Turn on Swagger UI in Development (and for the demo video).
2. Put a **summary** and **description** on every action: what it is for, which body it wants, and what it returns.
3. Annotate request DTOs so the schema shows required fields (`email`, `password`, `role`, event name, `sportType`, `categoryId`, finish time, and so on).
4. Document success and failure codes (`201`, `400`, `401`, `403`, `404`, `409`, `204`) so they appear in the UI, not only in this markdown file.
5. Enable cookie authentication in Swagger. After `POST /api/auth/login`, the next call in the same tab carries the session. That is how I will show that protected routes reject anonymous users and that the wrong role is blocked.

If a route cannot be seen or tried from Swagger, it is not finished.

---

## Unit tests I must pass (locally and on GitHub Actions)

The paper wants success **and** failure for authentication, event management, role rejection, and enrolments. Tests will use the in-memory (or SQLite) EF provider so CI does not need a live SQL Server.

| Area | Passing case | Failing case (expected) |
|---|---|---|
| Register | Organiser and Participant can register; password is stored hashed, not in plain text. | Duplicate email → **409**. Unknown role → **400**. |
| Login / session | Correct password creates a session; `/api/auth/session` then returns that user ID and role. | Wrong password → **401**. Protected route with no cookie → **401**. |
| Logout | After logout, a protected route returns **401**. | Calling logout with no session → **401**. |
| Events (Organiser) | Organiser creates, updates and deletes their own event (full CRUD). | Participant `POST /api/events` → **403**. Organiser edits someone else’s event → **403**. |
| Role rejection | Participant cannot capture results or list another event’s enrolments. Organiser cannot `POST /api/enrolments`. | Those calls return **403**, not **200**. |
| Enrolments | Participant enrols with a valid `eventId` + `categoryId`; the row stores participant, event and category; bib is set. | Second enrolment on the same event → **409**. Category from another event → **400**. Anonymous enrol → **401**. |

All of these must stay green in the GitHub Actions workflow on every push.

---

## Project shape for Part 2 (so the README and CI have something to check)

I will keep Part 1 artefacts and put the API next to them, roughly:

- `docs/` — ERD, this plan, `RaceDay.sql`
- `src/RaceDay.Api/` — ASP.NET Core Web API, EF Core, session + cookie auth, Swagger
- `tests/RaceDay.Api.Tests/` — xUnit tests for the table above
- `.github/workflows/` — build and `dotnet test` on every push
- `README.md` — how to restore, run SQL or migrate, run the API, open Swagger, what the two roles can do, CI screenshot, YouTube demo link

The database schema must stay aligned with the eight tables in Part 1. Code first does not mean I redesign the ERD; it means the C# entities produce the same keys, checks and relationships.

---

## References

Fielding, R.T. (2000) *Architectural styles and the design of network-based software architectures*. Doctoral dissertation. University of California, Irvine.

Sandhu, R.S., Coyne, E.J., Feinstein, H.L. and Youman, C.E. (1996) ‘Role-based access control models’, *IEEE Computer*, 29(2), pp. 38–47.

W3Schools (n.d.a) *HTTP status messages*. Available at: https://www.w3schools.com/tags/ref_httpmessages.asp (Accessed: 14 October 2026).


## Additional organiser convenience endpoint
| GET | `/api/events/mine` | Lists events belonging to the authenticated organiser | Organiser | None | 200 with own event list; 401/403 otherwise |
