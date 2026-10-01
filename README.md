# E-Invoice

Blazor Server (.NET 8) admin base for the E-Invoice system — the VIBS project (without Departments and
Application Modules) on **SQL Server**. UAE FTA / Peppol e-invoicing will be added later as new modules.

## Included
- Login / logout, Remember Me, lockout after 5 failed attempts, change password, 2FA pages
- Users (create, edit, activate/deactivate, reset password, roles, companies, "reported to")
- Roles and permission-based authorization (`Permissions.cs` is shown automatically in the Roles screen)
- Companies, Countries, Cities masters
- Customisable dashboard (widgets and quick links, saved per user)

## Solution layout
```
E-Invoice.Web             Blazor UI, layouts, pages, Identity account UI
E-Invoice.Application     DTOs and service interfaces
E-Invoice.Domain          Entities, permission registry
E-Invoice.Infrastructure  EF Core (SQL Server), Identity, service implementations
```
Earlier experiments are kept (not built) in `_archive/`: `firestore-version` and `postgres-version`.

## Run locally
1. SQL Server / SQL Express must be running. Check `ConnectionStrings:DefaultConnection` in
   `E-Invoice.Web/appsettings.json` (default: `.\SQLEXPRESS`, database `EInvoice`, Windows login).
2. Create the first migration once (needs `dotnet tool install --global dotnet-ef`):
   ```
   dotnet ef migrations add InitialCreate --project E-Invoice.Infrastructure --startup-project E-Invoice.Web --output-dir Persistence/Migrations
   ```
3. `dotnet run --project E-Invoice.Web`
   In Development the app applies migrations and seeds the roles and the first administrator:
   `admin@einvoice.local` / `Admin#12345` (see `appsettings.Development.json` — change it).

Schema changes later: run the same `dotnet ef migrations add <Name> ...` command and start the app.

## Moving to the cloud later
- The `Dockerfile` builds a container (works for Google Cloud Run, Azure, etc.).
- Login keys are stored in the database, so redeploys do not sign users out.
- On the server set: `ConnectionStrings__DefaultConnection`, and for the first start
  `Database__AutoMigrate=true`, `Seed__Enabled=true`, `Seed__AdminEmail`, `Seed__AdminPassword`
  (then set `Seed__Enabled=false`).
- Database options: Azure SQL / SQL Server on a VM, or Google Cloud SQL for SQL Server.

## Adding a module (e.g. Invoice)
1. Add `Permissions.Invoice` (`View/Create/Edit/Delete`) in `E-Invoice.Domain/Authorization/Permissions.cs`.
2. Add the entity, DTOs and `IInvoiceManagementService`.
3. Add the `DbSet`/mapping in `EInvoiceDbContext` and the service in `E-Invoice.Infrastructure/Services`,
   register it in `DependencyInjection.cs`, add a migration, then the pages and nav link.
