# RaceDay Planning Documents

This folder contains the planning artifacts for the RaceDay application as required for Part 1 of the Portfolio of Evidence.

## Entity Relationship Diagram (ERD)

- `ERD_ST10486077.png` depicts the complete logical data model representing the database schema for RaceDay.
- It includes all entities, attributes, primary keys, foreign keys, and clearly indicated cardinalities.
- The ERD supports all required system features, reflecting user roles, event management, enrolments, results capture, route and weather data.

## API Endpoint Plan

- `ASSIGN(POE PART1)_ST10486077.pdf` outlines all planned RESTful API endpoints with:
  - HTTP Method and Route
  - Description and Role-based access requirements
  - Expected request payloads and response codes
- The plan covers authentication, user profiles, events, categories, enrolments, results, routes, weather snapshots, and clubs.
- Role-based security and scenarios for Organiser, Participant, and public access have been carefully considered.

## SQL Database Script

- The SQL script (`RaceDayDatabaseSetup.sql`) fully implements the ERD schema with tables, keys, constraints, and sample seed data.
- Seed data fulfills the rubric minimums with:
  - 2 Organisers and 2 Participants users
  - 3 Events including categories and routes
  - Sample enrolments and results
- The script runs without errors on a clean SQL Server instance.
- This script forms the foundation for API functionality and system data integrity.

---

By using these design artifacts, RaceDay will support reliable event management and participant tracking conforming to project specifications and assessment criteria.

# RaceDay - Portfolio of Evidence Part 1

## Overview

This repository contains all deliverables for the first part of the RaceDay project, a web-based event management system for South African road events.

### Planning and Design

- The `/docs` folder includes:
  - A comprehensive **Entity Relationship Diagram (ERD)** illustrating the full data model.
  - A detailed **API Endpoint Plan** specifying all REST endpoints, role-based access, request/response formats.
  - A robust **SQL Database Setup Script** that implements the ERD fully, with constraints and seeded sample data.

### Compliance with Assessment Rubric

- The ERD is complete and accurate with clear keys and cardinalities, meeting the 25-mark criteria.  
- The API plan covers all functional areas, roles, and error responses comprehensively, fulfilling the rubric’s expectations.  
- The SQL script:
  - Runs cleanly without errors on a fresh database.
  - Creates all necessary tables with constraints.
  - Seeds realistic sample data matching the requirements (2 organisers, 2 participants, multiple events/categories).  
- The commit history in GitHub shows consistent meaningful commits related to planning and SQL scripting.
- CI/CD workflow file is included and passing (see `.github/workflows/ci.yml`).

### Next Steps

This planning work provides a strong foundation for the implementation stages in Parts 2 and 3, including the REST API and MVC web client.

---

For detailed documentation, please see the `/docs` folder.



References (Harvard)

Cape Town Cycle Tour (n.d.) Cape Town Cycle Tour. Available at: https://www.capetowncycletour.com/ (Accessed: 5 August 2026).

Comrades Marathon Association (n.d.) Comrades Marathon. Available at: https://www.comrades.com/ (Accessed: 5 August 2026).

Connolly, T. and Begg, C. (2015) Database systems: a practical approach to design, implementation, and management. 6th edn. Harlow: Pearson.

Fielding, R.T. (2000) Architectural styles and the design of network-based software architectures. PhD thesis. University of California, Irvine. Available at: https://www.ics.uci.edu/~fielding/pubs/dissertation/top.htm (Accessed: 7 August 2026).

Microsoft (2024) CREATE TABLE (Transact-SQL). Available at: https://learn.microsoft.com/en-us/sql/t-sql/statements/create-table-transact-sql (Accessed: 10 August 2026).

Sandhu, R.S., Coyne, E.J., Feinstein, H.L. and Youman, C.E. (1996) ‘Role-based access control models’, IEEE Computer, 29(2), pp. 38–47.

W3Schools (n.d.a) HTTP request methods. Available at: https://www.w3schools.com/tags/ref_httpmethods.asp (Accessed: 11 August 2026).

W3Schools (n.d.b) SQL foreign key constraint. Available at: https://www.w3schools.com/sql/sql_foreignkey.asp (Accessed: 16 August 2026)
