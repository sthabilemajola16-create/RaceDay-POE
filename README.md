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

For detailed documentation, please see the `/docs` folder.


References 

Cape Town Cycle Tour (n.d.) Cape Town Cycle Tour. Available at: https://www.capetowncycletour.com/ (Accessed: 5 August 2026).

Comrades Marathon Association (n.d.) Comrades Marathon. Available at: https://www.comrades.com/ (Accessed: 5 August 2026).

Connolly, T. and Begg, C. (2015) Database systems: a practical approach to design, implementation, and management. 6th edn. Harlow: Pearson.

Fielding, R.T. (2000) Architectural styles and the design of network-based software architectures. PhD thesis. University of California, Irvine. Available at: https://www.ics.uci.edu/~fielding/pubs/dissertation/top.htm (Accessed: 7 August 2026).

Microsoft (2024) CREATE TABLE (Transact-SQL). Available at: https://learn.microsoft.com/en-us/sql/t-sql/statements/create-table-transact-sql (Accessed: 10 August 2026).

Sandhu, R.S., Coyne, E.J., Feinstein, H.L. and Youman, C.E. (1996) ‘Role-based access control models’, IEEE Computer, 29(2), pp. 38–47.

W3Schools (n.d.a) HTTP request methods. Available at: https://www.w3schools.com/tags/ref_httpmethods.asp (Accessed: 11 August 2026).

W3Schools (n.d.b) SQL foreign key constraint. Available at: https://www.w3schools.com/sql/sql_foreignkey.asp (Accessed: 16 August 2026)

W3Schools (n.d.c) SQL CREATE TABLE. Available at: https://www.w3schools.com/sql/sql_create_table.asp (Accessed: 17 August 2026).

W3Schools (n.d.d) SQL PRIMARY KEY constraint. Available at: https://www.w3schools.com/sql/sql_primarykey.asp (Accessed: 18 August 2026).

W3Schools (n.d.e) SQL NOT NULL constraint. Available at: https://www.w3schools.com/sql/sql_notnull.asp (Accessed: 20 August 2026).

W3Schools (n.d.f) SQL INSERT INTO. Available at: https://www.w3schools.com/sql/sql_insert.asp (Accessed: 20 August 2026).

W3Schools (n.d.g) SQL UNIQUE constraint. Available at: https://www.w3schools.com/sql/sql_unique.asp (Accessed: 21 August 2026).

W3Schools (n.d.h) SQL DEFAULT constraint. Available at: https://www.w3schools.com/sql/sql_default.asp (Accessed: 24 August 2026).

W3Schools (n.d.i) SQL CHECK constraint. Available at: https://www.w3schools.com/sql/sql_check.asp (Accessed: 25 August 2026).
