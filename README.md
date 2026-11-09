# RaceDay – Race Event Management System

## 1. Project Overview

RaceDay is a web-based race event management system developed as part of the PROG6212 Portfolio of Evidence (POE).

The application provides a platform where participants can explore sporting events, register for available race categories and view their event entries and recorded results. Organisers can create and manage events, manage race categories, review participant enrolments and capture race results.

The project demonstrates the design, development, testing and implementation of a database-driven web application supported by a RESTful API.

## 2. Project Objectives

The main objectives of RaceDay are to:

- Provide an accessible platform for discovering sporting events.
- Support participant registration and authentication.
- Allow organisers to create and manage sporting events.
- Enable participants to enrol in available event categories.
- Support race result recording and viewing.
- Provide secure, role-based access to application features.
- Demonstrate database integration and REST API functionality.
- Provide a responsive and user-friendly interface.

## 3. Main Features

### Participant Features

- Register a new account and log in.
- Explore available sporting events.
- View event information and available categories.
- Enter available race categories.
- View confirmed event entries.
- View recorded race results, including finishing position and finish time.

### Organiser Features

- Access the organiser dashboard.
- Create and manage sporting events.
- Edit event information.
- Manage event categories.
- View participant enrolments.
- Capture race finishing times, positions and result statuses.

### API Features

- RESTful API endpoints.
- Database-backed application functionality.
- Authentication and role-based authorisation.
- Swagger API documentation.
- API integration with the web application.

## 4. Technologies Used

| Technology | Purpose |
|---|---|
| C# | Application programming language |
| ASP.NET Core | Web application and API development |
| .NET 8 | Application framework |
| ASP.NET Core MVC / Razor | Web interface |
| SQL Server | Database management |
| Entity Framework Core | Database access |
| Swagger / OpenAPI | API documentation and testing |
| HTML and CSS | Frontend structure and styling |
| JavaScript | Frontend interaction |
| Docker Compose | Database and service configuration |
| Git and GitHub | Version control and project hosting |
| Visual Studio 2022 | Development environment |

## 5. User Roles

RaceDay supports two main user roles:

**Participant**
- Browse events.
- Enter race categories.
- View event entries.
- View recorded race results.

**Organiser**
- Create and edit events.
- Manage categories.
- Review enrolments.
- Capture race results.

Access to protected functionality is controlled through authentication and role-based authorisation.

## 6. User Interface and Design

The RaceDay interface uses a purple glass-inspired visual theme with contrasting text, rounded cards and interactive navigation.

The design aims to provide:
- Clear navigation.
- Consistent page layouts.
- Readable content.
- Responsive presentation.
- Distinct participant and organiser workflows.
- Clear feedback during application interactions.

The interface includes login, registration, event exploration, event details, participant dashboards, organiser dashboards, category management and result capture pages.

## 7. Project Structure

The solution is organised into three main projects:

```text
RaceDay-POE/
│
├── src/
│   ├── RaceDay.Api/
│   │   ├── Controllers/
│   │   ├── Models/
│   │   ├── Services/
│   │   ├── Program.cs
│   │   └── Dockerfile
│   │
│   └── RaceDay.Web/
│       ├── Controllers/
│       ├── Models/
│       ├── Services/
│       ├── Views/
│       ├── wwwroot/
│       └── Program.cs
│
├── tests/
│   └── RaceDay.Api.Tests/
│
├── docs/
├── .github/
├── .env.example
├── .gitignore
├── docker-compose.yml
├── RaceDay.sln
└── README.md
```

## 8. Installation and Setup

### Requirements

Install the following software:

- Visual Studio 2022.
- .NET 8 SDK.
- SQL Server or the configured Docker database service.
- Git.
- Docker Desktop (if using Docker Compose).

### Clone the Repository

```bash
git clone https://github.com/sthabilemajola16-create/RaceDay-POE.git
```

Navigate into the project directory:

```bash
cd RaceDay-POE
```

### Restore Dependencies

```bash
dotnet restore
```

### Build the Solution

```bash
dotnet build
```

### Configure Environment Variables

Review `.env.example` and create a local `.env` file if you are using Docker Compose.

Configure your own SQL Server password and any required Azure Blob Storage connection settings locally.

**Security:** Never upload `.env`, actual passwords, private connection strings or Azure access keys to GitHub.

### Run the Application

Open `RaceDay.sln` in Visual Studio 2022.

Configure the startup projects according to your local solution setup:

- RaceDay.Api
- RaceDay.Web

Start the application using Visual Studio.

The API provides Swagger documentation when its Swagger interface is enabled.

## 9. Automated Testing

The project includes a separate automated test project:

```text
tests/RaceDay.Api.Tests/
```

Run the tests from the solution directory:

```bash
dotnet test
```

### Verified Test Results

The following results were obtained during the final local test run on 9 November 2026:

| Test measurement | Result |
|---|---:|
| Total tests | 19 |
| Passed | 19 |
| Failed | 0 |
| Skipped | 0 |
| Test duration | 8.4 seconds |

The complete solution also built successfully:

| Project | Build result |
|---|---|
| RaceDay.Web | Succeeded |
| RaceDay.Api | Succeeded |
| RaceDay.Api.Tests | Succeeded |

These results demonstrate that the solution compiled and that all 19 automated tests passed in the recorded test run.

## 10. API Documentation

RaceDay provides Swagger/OpenAPI documentation for exploring and testing the available API endpoints.

Swagger can be accessed through the API application's Swagger interface when the API is running and Swagger is enabled.

It provides an interface for inspecting API operations, reviewing request and response structures and testing supported endpoints.

## 11. Database

SQL Server is used for application data storage.

The database supports the information required for:
- User accounts and roles.
- Sporting events.
- Event categories.
- Participant enrolments.
- Race results.
- Related application records.

Database configuration should be supplied through the appropriate local application settings and environment variables.

## 12. Security Considerations

The application incorporates security measures including:

- User authentication.
- Role-based access control.
- Password hashing.
- Protected application operations.
- Anti-forgery protection for applicable form submissions.
- Environment-based configuration for sensitive settings.

Credentials and secrets must remain outside the public GitHub repository.

## 13. Screenshots and Demonstration

Screenshots of the actual running application should be added to this section to demonstrate:

1. Login and registration.
2. Participant dashboard.
3. Event exploration and event details.
4. Event category enrolment.
5. Organiser dashboard.
6. Event creation and category management.
7. Participant enrolments.
8. Race result capture.
9. Participant results dashboard.
10. Swagger API documentation.

A demonstration video should also be included when available.

## 14. Future Improvements

Potential improvements include:

- Online payment integration.
- Automated participant notifications.
- Enhanced event search and filtering.
- Additional accessibility improvements.
- More detailed race analytics and reporting.
- Expanded automated UI and integration testing.

## 15. Conclusion

RaceDay demonstrates the development of a database-driven race event management system using C#, ASP.NET Core, SQL Server and a RESTful API.

The application supports participant and organiser workflows, including event management, category enrolment and race result recording.

The final local verification completed successfully with all three solution projects building and all 19 automated tests passing.

## 16. Repository

**GitHub Repository:**  
https://github.com/sthabilemajola16-create/RaceDay-POE

**Module:** PROG6212  
**Project:** RaceDay – Race Event Management System

---

*This README describes the project implementation and the verified local build and test results. Screenshots, demonstration video and any additional evidence should be added from the actual running application.*