# E-Invoice

Blazor Server (.NET 8) admin base for the E-Invoice system — the VIBS project (without Departments and
Application Modules) on **PostgreSQL (Supabase)**. UAE FTA / Peppol e-invoicing will be added later as new modules.

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
E-Invoice.Infrastructure  EF Core (PostgreSQL / Npgsql), Identity, service implementations
```
Earlier experiments are kept (not built) in `_archive/`: `firestore-version` and `postgres-version`.

## Run locally
1. Store the Supabase connection string in user-secrets (never commit it). Use the **Session pooler**
   string from Supabase > Connect (IPv4, port 5432):
   ```
   dotnet user-secrets --project E-Invoice.Web set ConnectionStrings:DefaultConnection "Host=aws-0-<region>.pooler.supabase.com;Port=5432;Database=postgres;Username=postgres.<project-ref>;Password=<password>;SSL Mode=Require"
   ```
2. `dotnet run --project E-Invoice.Web`
   In Development the app applies migrations and seeds the roles and the first administrator:
   `admin@einvoice.local` / `Admin#12345` (see `appsettings.Development.json` — change it).

Schema changes later: run
`dotnet ef migrations add <Name> --project E-Invoice.Infrastructure --startup-project E-Invoice.Web --output-dir Persistence/Migrations`
and start the app.

## Moving data from the old SQL Server database
```
dotnet ef database update --project E-Invoice.Infrastructure --startup-project E-Invoice.Web --connection "<supabase>"
dotnet run --project tools/SqlServerToPostgres -- "Server=.\SQLEXPRESS;Database=EInvoice;Trusted_Connection=True;TrustServerCertificate=True" "<supabase>"
```
The tool copies all rows (users keep their passwords) and refuses to run if the target tables already contain rows.

## Moving to the cloud later
- The `Dockerfile` builds a container (works for Google Cloud Run, Render, Railway, Azure, etc.).
  Blazor Server needs a long-running server with WebSockets, so serverless hosts such as Vercel cannot run it.
- Login keys are stored in the database, so redeploys do not sign users out.
- On the server set: `ConnectionStrings__DefaultConnection`, and for the first start
  `Database__AutoMigrate=true`, `Seed__Enabled=true`, `Seed__AdminEmail`, `Seed__AdminPassword`
  (then set `Seed__Enabled=false`).
- Database: Supabase PostgreSQL (session pooler connection string).

## Adding a module (e.g. Invoice)
1. Add `Permissions.Invoice` (`View/Create/Edit/Delete`) in `E-Invoice.Domain/Authorization/Permissions.cs`.
2. Add the entity, DTOs and `IInvoiceManagementService`.
3. Add the `DbSet`/mapping in `EInvoiceDbContext` and the service in `E-Invoice.Infrastructure/Services`,
   register it in `DependencyInjection.cs`, add a migration, then the pages and nav link.
4. Deleting: call `DeleteGuard.DeleteAsync(...)` in the service's delete method. New foreign keys are `Restrict`
   automatically (see the end of `EInvoiceDbContext.OnModelCreating`), so a record that is still referenced
   (e.g. a customer used by an invoice) cannot be deleted and the user sees what is still connected.
