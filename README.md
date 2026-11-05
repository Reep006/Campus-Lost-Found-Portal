# Campus Lost & Found Portal

A web portal where students can report lost and found items on campus.

## Features

- Report a **lost** item or a **found** item (with optional photo upload)
- Search and filter items by name, description and category
- Automatic matching of lost items with found items
- Item details page with edit, delete and "mark as returned"
- Admin login and dashboard for managing items
- First-time admin setup page

## Tech Stack

- ASP.NET MVC 5 (.NET Framework 4.8.1)
- C# and Razor views
- SQL Server LocalDB (ADO.NET)
- Bootstrap 5.2.3, jQuery 3.7.0

## Project Structure

```
CampusLostFound/
  App_Start/     Routing, filters, bundles
  Controllers/   Home, Items, Admin, AdminSetup
  Models/        Item, Admin, ItemRepository, MatchService
  Views/         Razor views
  Content/       CSS files
  Scripts/       JavaScript libraries
  Uploads/       Uploaded item images (not tracked)
docs/            Setup and user guides
```

## Team

| Member   | Role                        |
|----------|-----------------------------|
| Reep     | Backend, models, controllers |
| Jeel     | Views and UI layout          |
| Utkarsh  | Styling, forms and docs      |
