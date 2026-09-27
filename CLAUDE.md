# Cinema Showtimes API

Take-home assessment: REST API for a cinema (movies, showtimes, seat reservations with 10-min expiry, confirmation).
Core scope: US-1..US-5. Optional: US-6 (contiguous seats), US-7 (concurrency safety).

## Stack
- .NET 10, ASP.NET Core Web API (controllers), EF Core + SQLite (file `cinema.db`, migrations)
- OpenAPI: Microsoft.AspNetCore.OpenApi + Scalar
- FluentValidation (request DTOs only)
- Tests: xUnit, NSubstitute, Shouldly, Microsoft.AspNetCore.Mvc.Testing, FakeTimeProvider
- Time: always inject `TimeProvider`, never `DateTime.UtcNow` / `DateTimeOffset.UtcNow`
- NOT used: Redis, MediatR, AutoMapper, LanguageExt, Repository/UnitOfWork patterns
- Do not add packages outside this list without asking.

## Solution layout
- `src/Cinema.Domain` — entities, enums, domain exceptions, domain logic. No dependencies, no EF attributes.
- `src/Cinema.Application` — service interfaces + implementations, DTOs (records), validators, `IAppDbContext`.
  References only `Microsoft.EntityFrameworkCore` (never a provider package).
- `src/Cinema.Infrastructure` — `AppDbContext : IAppDbContext`, `IEntityTypeConfiguration<T>` classes, migrations, seed data, DI extension.
- `src/Cinema.Api` — controllers, exception handler (ProblemDetails), DI composition, OpenAPI.
- `tests/Cinema.UnitTests`, `tests/Cinema.IntegrationTests`

## Architecture rules
- Controllers contain no business logic; they call Application services via interfaces.
- `IAppDbContext` is the unit of work; no repositories.
- Domain entities: private setters, private parameterless ctor for EF, static factory methods for creation, collections exposed as `IReadOnlyCollection<T>` over a private field.
- All EF mapping lives in `IEntityTypeConfiguration<T>` classes in Infrastructure.
- Timestamps are UTC `DateTime` (SQLite provider can't compare/order `DateTimeOffset` server-side).
- Enums stored as strings.
- Guid ids generated in the domain with `Guid.CreateVersion7()`; configure `ValueGeneratedNever()`.

## Domain model
- Movie: Id (Guid), Title, Category, Year, DurationMinutes
- Auditorium: Id (Guid), Name, Seats
- Seat: Id (Guid), AuditoriumId, Row (int), Number (int); unique (AuditoriumId, Row, Number)
- Showtime: Id (Guid), MovieId, AuditoriumId, StartTime (UTC)
- Reservation: Id (Guid, = customer reference), ShowtimeId, CreatedAt, ExpiresAt (= CreatedAt + 10 min), Status (Pending | Confirmed)
- ShowtimeSeat: PK (ShowtimeId, SeatId), Status (Free | Reserved | Sold), ReservationId?, Version (int, concurrency token, incremented manually)
- A Reserved seat whose reservation is expired is treated as Free. No background jobs.

## Error mapping
400 validation, 404 not found, 409 conflict (seat taken, reservation expired/already confirmed, concurrency conflict). All errors as ProblemDetails; unknown → 500 without details.

## Conventions
- Code, comments, commit messages in English.
- Build must stay green with TreatWarningsAsErrors. Do not suppress warnings globally.
- Common build props live in `Directory.Build.props`; do not re-add TargetFramework/Nullable/ImplicitUsings to csproj.
- Tests: AAA, one behavior per test, names `Method_Scenario_Expected`.
- Work milestone by milestone. Do not implement features from later milestones.
- Do not commit; stop after the task and summarize changes for review.

## Commands (macOS, zsh)
- Build: `dotnet build`
- Test: `dotnet test`
- Run: `dotnet run --project src/Cinema.Api`
- Add migration: `dotnet ef migrations add <Name> -p src/Cinema.Infrastructure -s src/Cinema.Api -o Persistence/Migrations`
- dotnet-ef is a local tool: run `dotnet tool restore` if missing.
