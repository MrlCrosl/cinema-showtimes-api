# Cinema Showtimes API

[![CI](https://github.com/MrlCrosl/cinema-showtimes-api/actions/workflows/ci.yml/badge.svg)](https://github.com/MrlCrosl/cinema-showtimes-api/actions/workflows/ci.yml)

A REST API for a small cinema: movies, showtimes in an auditorium, seat reservations that hold seats for 10 minutes,
and confirmation of a reservation. Built with .NET 10, ASP.NET Core controllers, EF Core and SQLite.

Implemented user stories: US-1 create a movie, US-2 create a showtime, US-3 reserve seats, US-4 confirm a reservation, US-5 persistence (the SQLite file survives restarts), US-6 contiguous seats, US-7 safe concurrent requests.

## Run locally

Requires the .NET 10 SDK.

```
dotnet run --project src/Cinema.Api
```

The app listens on http://localhost:5177 (the `http` launch profile; the `https` profile adds https://localhost:7103).
In the Development environment the API reference is at http://localhost:5177/scalar/v1 and the OpenAPI document at
http://localhost:5177/openapi/v1.json.

On startup the app creates the SQLite file `src/Cinema.Api/cinema.db`, applies migrations and seeds one auditorium
("Hall 1", 5 rows of 8 seats) and 5 movies. To reset, stop the app and delete `src/Cinema.Api/cinema.db`.

`src/Cinema.Api/Cinema.Api.http` contains ready-made requests for every endpoint, including the error cases; run them
top to bottom in an editor that supports .http request variables (for example the VS Code REST Client).

## Run with Docker

```
docker build -t cinema-api .
docker run --rm -p 8080:8080 -v cinema-data:/app/data -e ASPNETCORE_ENVIRONMENT=Development cinema-api
```

The API is then at http://localhost:8080. The SQLite file lives in the named volume `cinema-data`, so data survives
container restarts. `ASPNETCORE_ENVIRONMENT=Development` is set because the OpenAPI document and the Scalar page are
mapped in Development only; omit it and the API still works without them. The container runs as the base image's
non-root user over plain HTTP on port 8080.

## Tests

```
dotnet test
```

Unit tests (`tests/Cinema.UnitTests`) cover the domain rules without a database: reservation expiry and confirmation,
seat state transitions, the contiguous block finder and the request validators. Integration tests
(`tests/Cinema.IntegrationTests`) run the real HTTP pipeline through `WebApplicationFactory`, including migrations
and seed data, and cover every endpoint, expiry, error mapping and the concurrency scenarios.

Application services are tested through the HTTP pipeline against real SQLite rather than with a mocked DbContext,
because the behavior that matters here (queries, the concurrency token, one SaveChanges per operation) lives in the
database.

Each integration test gets its own temporary SQLite file rather than an in-memory database with one shared
connection, because the concurrency tests need real separate connections. Time is a `FakeTimeProvider`, so expiry is
tested by advancing the clock.

## Endpoints

| Method | Route | Purpose | Status codes |
|---|---|---|---|
| POST | /api/movies | Create a movie | 201, 400 |
| GET | /api/movies | List movies, ordered by title | 200 |
| GET | /api/movies/{id} | Get a movie | 200, 404 |
| POST | /api/showtimes | Create a showtime | 201, 400, 404, 409 |
| GET | /api/showtimes | List showtimes, ordered by start time | 200 |
| GET | /api/showtimes/{id} | Get a showtime | 200, 404 |
| GET | /api/showtimes/{id}/seats | Seat map with Free, Reserved or Sold per seat | 200, 404 |
| POST | /api/reservations | Reserve specific seats (up to 10) | 201, 400, 404, 409 |
| POST | /api/reservations/contiguous | Reserve a block of adjacent seats in one row | 201, 400, 404, 409 |
| GET | /api/reservations/{reference} | Get a reservation; state is Pending, Confirmed or Expired | 200, 404 |
| POST | /api/reservations/{reference}/confirm | Confirm a reservation; seats become Sold | 200, 404, 409 |

A reservation response contains `reference`, `state`, `showtimeId`, `startTime`, `movie` (`id`, `title`),
`auditorium` (`id`, `name`), `seatsCount`, `seats` (`seatId`, `row`, `number`), `createdAt` and `expiresAt`.

All errors are RFC 9457 ProblemDetails with a `traceId`. 400 is validation (field errors under `errors`, keys in
camelCase), 404 is an unknown movie, showtime, seat map or reservation, 409 is a conflict: overlapping showtime,
showtime already started, seat taken, no contiguous block, reservation expired or already confirmed, or a lost
concurrent update. Unexpected errors return 500 without details.

Validation limits: movie title up to 200 characters, category up to 100, year from 1888 to the current year plus 5,
duration 1 to 600 minutes; showtime start must be in the future; 1 to 10 seats per reservation, no duplicates.

## Design decisions

- **Layered projects.** `Cinema.Domain` (entities, rules, no dependencies), `Cinema.Application` (services, DTOs,
  validators, `IAppDbContext`), `Cinema.Infrastructure` (EF Core mappings, migrations, seed), `Cinema.Api`
  (controllers, error handling, OpenAPI). Controllers only call application services.
- **No repositories.** The `DbContext` is the unit of work behind `IAppDbContext`; services write LINQ against it and
  call `SaveChangesAsync` once per operation, so each operation is atomic.
- **Expiry is computed on read.** A reservation is expired when `now >= CreatedAt + 10 min` and it is still pending.
  The seat map and the reserve check treat such holds as free. There are no background jobs and nothing is deleted.
- **SQLite.** A single file, migrated on startup, is enough for the scope and makes the app self-contained. Timestamps
  are stored as UTC `DateTime` because the SQLite provider cannot compare `DateTimeOffset` server-side.
- **FluentValidation and ProblemDetails.** Request DTOs are validated by injected validators inside the services; a
  single exception handler maps validation, not-found, conflict and unexpected errors to ProblemDetails.
- **TimeProvider.** Every "now" comes from an injected `TimeProvider`, which is what lets the tests control expiry.
- **Aggregates reference each other by id only.** `ShowtimeSeat` has a `ReservationId`, not a `Reservation`
  navigation; seat holders are loaded in one batched second query (no N+1).
- **Indexes verified with `EXPLAIN QUERY PLAN`.** Every reservation and write path is an index `SEARCH`; the showtime
  overlap check is covered by `IX_Showtimes_AuditoriumId_StartTime` (equality on `AuditoriumId`, range on `StartTime`).

## Concurrency (US-7)

Each seat of a showtime is a `ShowtimeSeat` row with an integer `Version` that is an EF Core concurrency token and is
incremented by the domain on every state change. Reserving reads the rows, checks availability, calls `Reserve` on
each and saves; the generated `UPDATE` includes `WHERE Version = <value read>`. If another request changed the row in
between, zero rows are affected, EF Core throws `DbUpdateConcurrencyException`, and the service turns it into a 409
("One or more seats were taken by another request. Please retry."). The winner is whichever request commits first;
nothing is partially applied because the reservation and all seat updates are in one `SaveChangesAsync`. SQLite
allows only one writer at a time, but that alone does not prevent a lost update: both requests can read the seat as
free before either of them writes. The token is what turns the second write into a conflict.

This is tested two ways: HTTP race loops (two clients reserve the same seat 20 times on fresh showtimes and must get
exactly one 201 and one 409; two clients request 4 contiguous seats 10 times and their seat sets must never overlap)
and a deterministic test that loads the same seat into two `DbContext` instances, saves the first, and asserts the
second save throws `DbUpdateConcurrencyException`.

The alternative is pessimistic locking: `SELECT ... FOR UPDATE` on PostgreSQL or `WITH (UPDLOCK)` on SQL Server
inside a transaction, which serialises competing requests instead of failing one. Optimistic concurrency was chosen
because it needs no database-specific locking (SQLite has none of the above), conflicts on the same seat are rare and
cheap to retry, and the token makes the read-then-write window explicit and testable.

## Assumptions and edge cases

- A reservation expires at `CreatedAt + 10 minutes`; at exactly 10 minutes it is already expired.
- Reserving is rejected once the showtime has started (`now >= StartTime`). Confirming a live reservation after the
  start is allowed.
- An expired hold counts as free on read and is never deleted. If its seats are reserved again, `GET` of the old
  reservation shows fewer seats (the seats now point at the new reservation).
- At most 10 seats per reservation. For the contiguous endpoint, a count of 9 or 10 is valid input but always returns
  409 with the seeded 8-seat rows, because the validator does not know the hall layout.
- Contiguous selection is first fit: the lowest row, leftmost block whose seats are all available. There is no
  server-side retry; a lost race returns 409 and the client retries.
- An unknown seat id for the showtime returns 400; a taken seat returns 409 naming the seat by row and number.
- Two showtimes in the same auditorium may not overlap; the check compares `[start, start + duration)` against
  existing showtimes and rejects with 409. This check is read-then-insert without a lock or a database constraint, so
  two overlapping showtimes created at the same instant can both succeed. Showtime creation is a staff action, and
  guarding it was left out of scope.

## Out of scope and next steps

Authentication for staff endpoints (movies and showtimes), payments, auditorium management, best-fit seat selection,
server-side retry on a lost race, cleanup of expired holds, PostgreSQL.

## Use of AI tools

The code was implemented with Claude Code working as an agent from milestone specifications. The design decisions, the
specifications and the review of every diff are the author's.
