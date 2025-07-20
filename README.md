# Invoice Web App
A simple web application built with ASP.NET Core MVC to manage invoices. 
This project demonstrates basic CRUD (Create, Read, Update, Delete) operations,
 user authentication (login, registration, email verification), and multi-language support.

## Features
**User Authentication:**
    -   User Registration with email verification.
    -   User Login with Captcha.
    -   Password hashing using BCrypt.NET.

**Invoice Management:**
usign Crud operation in Invoice like ( List all invoices- Create new invoices-View detailed invoice information-Edit invoices- Delete invoices)

**Email Integration:**
    -   Sends email verification links upon registration.
    -   Sends welcome emails upon successful email verification.
    -   Sends notification emails upon invoice creation.

 **Multi-Language Support:**
 Toggle between English and Arabic  languages.

**N-Tier Architecture:**
    -   Separation of concerns using Controller, Service, Repository, and Unit of Work layers.

**Data Access:**
    -   Entity Framework Core Code First for database interactions.
    -   Configured for SQL Server (without Identity columns for primary keys).


## Prerequisites
Before you begin, ensure you have met the following requirements:
-   [.NET 8.0 SDK](https://dotnet.microsoft.com/download/dotnet/8.0) installed.
-   [Visual Studio 2022](https://visualstudio.microsoft.com/vs/) (Community Edition or higher) or a compatible IDE like JetBrains Rider.
-   [SQL Server](https://www.microsoft.com/en-us/sql-server/sql-server-downloads) (Express LocalDB is sufficient for local development).
-   A Gmail account with [2-Step Verification enabled](https://myaccount.google.com/security) to generate an [App Password](https://myaccount.google.com/apppasswords) for sending emails.


## Getting Started
Follow these steps to get your development environment set up.

### 1. Clone the Repository
### 2. Database Setup  --------> Update appsettings.json: ------> Update the ConnectionStrings:sc value to point to your local SQL Server instance.
### 3.Application Base URL ---->  Ensure AppSettings:BaseUrl matches the HTTPS URL your application will run on locally (e.g., https://localhost:7001)
### 4.Apply Migrations:      4.1 -Add a new migration:  4.2 -Update the database:

