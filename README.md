# URL Shortener

Test assignment: a link shortening service built with ASP.NET Core MVC, Angular and EF Core (Code First).

## Stack

| Part | Technology |
|---|---|
| Backend | ASP.NET Core 9 MVC, Razor Views |
| Table frontend | Angular 22 (standalone components, signals, zoneless) |
| Data | EF Core 9, SQL Server, Code First with migrations |
| Authentication | ASP.NET Core Identity, cookies, Admin / User roles |
| Tests | xUnit, NSubstitute, EF Core in-memory SQLite |

## Layout

```
UrlShortener.sln
├── UrlShortener.Domain/          entities, domain services, interfaces — no infrastructure dependencies
├── UrlShortener.Infrastructure/  EF Core: DbContext, configurations, migrations, repository, seeding
├── UrlShortener.Web/             controllers, Razor views, API
│   └── ClientApp/                Angular application of the table (built into wwwroot/app)
└── UrlShortener.Tests/           unit tests
```

## Running

You need the .NET SDK 9, Node.js 24.15+ and SQL Server (LocalDB will do).

```bash
dotnet run --project UrlShortener.Web
```

On the first run the Angular dependencies are installed automatically, the frontend is built into
`wwwroot/app`, migrations are applied and the database is filled with the starting data. There is no
separate frontend build step; pass `-p:SkipClientAppBuild=true` to skip it.

The connection string is `ConnectionStrings:Default` in [appsettings.json](UrlShortener.Web/appsettings.json).

### Accounts

| Login | Password | Role |
|---|---|---|
| `admin` | `Admin123$` | Admin |
| `user` | `User123$` | User |

### Tests

```bash
dotnet test
```

## Features

| Page | Access |
|---|---|
| Sign in (`/Account/Login`) | everyone |
| Links table (`/`) — Angular | everyone can view; adding and deleting require authentication |
| Link details (`/ShortUrls/Details/{id}`) | authenticated users only |
| About (`/About`) | everyone can read, only Admin can edit |
| Redirect (`/s/{code}`) | everyone |

Table permissions: anonymous users can only browse; a regular user adds links and deletes their own;
an administrator deletes any. Changes appear without reloading the page — Angular patches the list
from the API response.

## Shortening algorithm

Every link is assigned a number from a SQL Server sequence (`ShortUrlCodeSequence`), which is then
encoded in base 62 over the alphabet `0-9 A-Z a-z`. For example, `1000000` becomes `4C92`.

Why this way:

- the sequence hands out values atomically, so codes are unique by construction — neither a retry
  loop nor a check for a taken code is needed;
- base 62 encoding is a bijection, so uniqueness of the numbers carries over to the codes, while
  length grows logarithmically: `62^4` already covers more than 14 million addresses;
- starting the sequence at 1,000,000 yields four-character codes right away and does not reveal how
  many links the system holds.

The address is normalized before it is stored: scheme and host are lowercased, the default port and
the trailing slash at the root are dropped, and a missing scheme defaults to `https`. Uniqueness is
enforced by a unique index over the normalized address, so a repeated shortening is rejected even
under concurrent requests — the index violation is translated into a domain exception and returned
to the client as `409 Conflict`.

## Decisions worth explaining

- **The `/s/` prefix for short links.** A code at the root (`/{code}`) would collide with `/About`
  and `/Account/Login`.
- **Angular inside the Web project.** The application is built into `wwwroot/app` and mounted into
  the Razor view of the table, so the frontend and the backend share one origin: cookie
  authentication works without CORS or JWT, and the whole solution starts with a single command.
- **The antiforgery token via `/api/session`.** Angular's built-in XSRF mechanism reads the token
  from a cookie, but ASP.NET Core puts it into an HttpOnly one. The token arrives together with the
  session data and travels back in the `X-CSRF-TOKEN` header.
- **`IClock` and `ICodeSequence` behind interfaces.** Time and the sequence call are exactly what
  cannot be asserted directly in tests; behind an interface they are substitutable.
