# RaceDay – Race Event Management System

**Module:** PROG6212 – Programming 2B  
**Project:** RaceDay Portfolio of Evidence (POE)  
**GitHub Repository:** https://github.com/sthabilemajola16-create/RaceDay-POE

## 1. Project Overview

RaceDay is a web-based race event management system developed as part of the PROG6212 Portfolio of Evidence (POE).

The application provides a platform where participants can explore sporting events, register for available race categories and view their event entries and recorded results.

Organisers can create and manage events, manage race categories, review participant enrolments and capture race results.

The solution consists of an ASP.NET Core Web API, an ASP.NET Core MVC web application and an automated testing project.

## 2. Project Objectives

The main objectives of RaceDay are to:

- Provide an accessible platform for discovering sporting events.
- Allow participants to register and enrol in race categories.
- Allow organisers to create and manage sporting events.
- Support participant enrolment and bib number allocation.
- Allow organisers to capture race results.
- Provide participants with access to their event history and results.
- Demonstrate secure authentication and role-based access control.
- Implement a database-driven application supported by a RESTful API.

## 3. Main Features

### 3.1 Participant Features

- User registration and login.
- Browse available sporting events.
- View event information and available categories.
- Enrol in event categories.
- View confirmed event entries.
- View allocated bib numbers.
- View recorded race results.

### 3.2 Organiser Features

- Organiser login.
- Create sporting events.
- Edit and manage existing events.
- Add and manage race categories.
- View participant enrolments.
- Review participant categories and bib numbers.
- Capture finishing times and positions.
- Update race result statuses.

### 3.3 Event Management

RaceDay supports different sporting event types, including:

- Running.
- Walking.
- Cycling.

Each event can contain information such as its name, venue, location, date, route and available categories.

## 4. User Roles

| User Role | Responsibilities |
|---|---|
| Participant | Browse events, enrol in categories and view results. |
| Organiser | Create events, manage categories, review enrolments and capture results. |
| Public Visitor | Access public event information and registration pages. |

The application uses authentication and role-based authorisation to control access to protected functionality.

## 5. Technologies Used

| Technology | Purpose |
|---|---|
| C# | Application programming language |
| .NET 8 | Development platform |
| ASP.NET Core Web API | Backend services and API endpoints |
| ASP.NET Core MVC | Web application interface |
| SQL Server | Relational database |
| Swagger / OpenAPI | API documentation and testing |
| xUnit | Automated testing |
| HTML | Web page structure |
| CSS | Application styling |
| JavaScript | Frontend interaction |
| Git | Version control |
| GitHub | Source code hosting |
| Docker Compose | Container configuration |

## 6. Project Structure

The solution is organised into the following structure:

```text
RaceDay-POE/
│
├── .github/
│
├── docs/
│   └── Project documentation
│
├── src/
│   ├── RaceDay.Api/
│   │   └── Backend API
│   │
│   └── RaceDay.Web/
│       └── MVC web application
│
├── tests/
│   └── RaceDay.Api.Tests/
│       └── Automated tests
│
├── .env.example
├── .gitignore
├── docker-compose.yml
├── RaceDay.sln
└── README.md
```

## 7. System Requirements

The following software is required to run the application:

- Windows 10 or Windows 11.
- .NET 8 SDK.
- Visual Studio 2022 with the ASP.NET and web development workload.
- SQL Server.
- Git for cloning the repository.
- Docker Desktop if using Docker Compose.

## 8. Installation and Setup

### Step 1: Clone the Repository

Open PowerShell or a terminal and run:

```bash
git clone https://github.com/sthabilemajola16-create/RaceDay-POE.git
```

Navigate into the project folder:

```bash
cd RaceDay-POE
```

### Step 2: Open the Solution

Open the following file using Visual Studio 2022:

```text
RaceDay.sln
```

Allow Visual Studio to restore the required NuGet packages.

### Step 3: Configure the Database

Configure the SQL Server connection using the settings expected by the API project.

Use your own local database connection details.

Check the database setup instructions and scripts included in the repository before applying any database changes.

### Step 4: Configure Environment Settings

The repository contains an `.env.example` file.

Use it as a reference when creating your local configuration.

Do not upload passwords, database credentials, connection strings containing secrets or API keys to GitHub.

### Step 5: Restore Dependencies

From the solution directory, run:

```bash
dotnet restore
```

### Step 6: Build the Application

Run:

```bash
dotnet build
```

### Step 7: Run the Application

Start the API:

```bash
dotnet run --project src/RaceDay.Api
```

Open a second terminal and start the web application:

```bash
dotnet run --project src/RaceDay.Web
```

Use the local URLs displayed in the terminal when the applications start.

The MVC application must use the correct API base URL.

Alternatively, configure Visual Studio to start both `RaceDay.Api` and `RaceDay.Web` together.

## 9. API Documentation

RaceDay provides API documentation through Swagger/OpenAPI.

When the API is running, open:

```text
http://localhost:5188/swagger
```

If the API uses a different port, use the Swagger URL displayed by the application.

Swagger allows developers to explore available endpoints and test API requests.

## 10. Database Management

RaceDay uses SQL Server for relational data storage.

The database supports the application's main operations, including:

- User accounts.
- Organiser and participant roles.
- Sporting events.
- Race categories.
- Participant enrolments.
- Bib numbers.
- Race results.

Database configuration must match the SQL Server instance used when running the application.

## 11. Automated Testing

The project includes an automated test project:

```text
tests/RaceDay.Api.Tests
```

The tests cover application functionality, including authentication, enrolments and event and role-related behaviour.

To execute the tests, run:

```bash
dotnet test
```

### Verified Test Results

The test execution captured during development produced the following results:

| Test Measure | Result |
|---|---:|
| Total Tests | 19 |
| Passed | 19 |
| Failed | 0 |
| Skipped | 0 |
| Duration | 8.4 seconds |

The development verification also showed successful builds for:

- RaceDay.Web
- RaceDay.Api
- RaceDay.Api.Tests

All three projects successfully built targeting .NET 8.

These results reflect the recorded local test run. Run the commands again after making changes to verify the latest version.

## 12. Security

Security is an important part of the RaceDay application.

The system includes:

- User authentication.
- Role-based access control.
- Protected organiser functionality.
- Participant access restrictions.
- Password protection.
- Environment-based configuration for sensitive settings.

Sensitive configuration files must remain local and must not be committed to the public GitHub repository.

## 13. User Interface and Design

RaceDay uses a purple glass-inspired visual theme.

The interface includes:

- Navigation between application sections.
- Registration and login pages.
- Participant dashboard.
- Organiser dashboard.
- Event listing and details pages.
- Category management pages.
- Enrolment and result management pages.

The design aims to provide a consistent and accessible experience across the main application workflows.

## 14. Screenshots and Demonstration Evidence

The following screenshots should be included as evidence of the working application:

1. Registration and login page.
2. Participant dashboard.
3. Available sporting events.
4. Event details and categories.
5. Successful participant enrolment.
6. Organiser dashboard.
7. Event creation.
8. Category management.
9. Participant enrolment management.
10. Race result capture.
11. Participant dashboard displaying recorded results.
12. Swagger API documentation.
13. Successful application build.
14. Successful automated test execution.

Only genuine screenshots captured from the running application should be used.

### Demonstration Video

A demonstration video should show:

- Participant registration and login.
- Browsing available events.
- Enrolling in an event category.
- Viewing participant entries.
- Organiser login.
- Creating and managing an event.
- Managing race categories.
- Reviewing enrolments.
- Capturing race results.
- Viewing recorded results as a participant.
- Swagger API documentation.
- Automated test results.

Add the actual video link here after uploading the completed demonstration:

**Video Link:** To be added.

## 15. Further Verification

The following items should be checked in the final submission environment:

- [ ] Fresh installation using the README instructions.
- [ ] Database configuration and connectivity.
- [ ] Participant authentication.
- [ ] Organiser authentication.
- [ ] Role-based access restrictions.
- [ ] Event creation and editing.
- [ ] Category creation and management.
- [ ] Participant enrolment persistence.
- [ ] Result capture and participant result visibility.
- [ ] Keyboard navigation and accessibility.
- [ ] Form labels and visible focus indicators.
- [ ] Colour contrast and WCAG 2.0 considerations.
- [ ] Docker configuration, if included in the final demonstration.
- [ ] Genuine application screenshots.
- [ ] Final demonstration video.

## 16. References

Fielding, R.T. (2000) *Architectural styles and the design of network-based software architectures*. Doctoral dissertation. University of California, Irvine.

Microsoft (n.d.) *ASP.NET Core documentation*. Available at: https://learn.microsoft.com/aspnet/core

Microsoft (n.d.) *.NET documentation*. Available at: https://learn.microsoft.com/dotnet

Microsoft (n.d.) *SQL Server documentation*. Available at: https://learn.microsoft.com/sql/

Sandhu, R.S., Coyne, E.J., Feinstein, H.L. and Youman, C.E. (1996) ‘Role-based access control models’, *IEEE Computer*, 29(2), pp. 38–47.

World Wide Web Consortium (W3C) (2008) *Web Content Accessibility Guidelines (WCAG) 2.0*. Available at: https://www.w3.org/TR/WCAG20/

---

**Project:** RaceDay – Race Event Management System  
**Module:** PROG6212  
**Repository:** https://github.com/sthabilemajola16-create/RaceDay-POE

This README documents the RaceDay project, its features, technologies, setup instructions and development verification results.