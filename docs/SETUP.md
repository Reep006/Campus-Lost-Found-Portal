# Setup Guide

## Requirements
- Visual Studio 2022 (ASP.NET and web development workload)
- .NET Framework 4.8.1 developer pack
- SQL Server LocalDB

## Steps
1. Clone the repository.
2. Open `CampusLostFound.slnx` in Visual Studio.
3. Restore NuGet packages (right-click solution -> **Restore NuGet Packages**).
4. Create the `CampusLostFoundDB` database in LocalDB and the `Items` / `Admins` tables.
5. Check the `CampusLostFoundConnection` connection string in `Web.config`.
6. Press **F5** to run with IIS Express.
7. Open `/AdminSetup/Create` once to create the first admin account.
