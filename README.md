# ChatApp Lab

Spec Kit artifacts live in `specs/001-facebook-style-chat/`.

## Projects

- `ChatApp.API` - ASP.NET Core API, SignalR, JWT auth, EF Core, PostgreSQL.
- `ChatApp.UI` - React/Vite frontend.
- `ChatApp` - Blazor prototype, kept for reference.

## Docker Dev Run

Start Docker Desktop first, then run the full stack. The compose file is configured for development hot reload:

```powershell
docker compose up --build
```

Save changes in `ChatApp.UI/src` and Vite will update the browser immediately. Save changes in `ChatApp.API` and `dotnet watch` will hot reload or restart the API automatically.

PostgreSQL is exposed on host port `5433` to avoid conflicts with any local PostgreSQL service on `5432`.

Open:

```text
Frontend: http://localhost:5173
API:      http://localhost:5185
DB:       localhost:5433
```

## Local Non-Docker Run

```powershell
docker compose up -d postgres
dotnet run --project ChatApp.API\ChatApp.API.csproj
cd ChatApp.UI
npm run dev -- --host 127.0.0.1 --port 5173
```

Default admin:

```text
Email: admin@chat.local
Password: Admin@123456
```

The app applies EF Core migrations and seeds roles/admin on startup in Development.

If hot reload seems stale after dependency changes, rebuild the dev containers:

```powershell
docker compose up -d --build --force-recreate frontend api
```

## Features

- Email/password login with JWT generation
- Admin/User roles
- One shared SignalR chat room
- Text and emoji messages
- Local file/image/video upload
- Online/offline presence
- Typing indicator
- Seen count
- Sender-only recall
- Admin message delete

## Verification

The Docker PostgreSQL container, EF Core migration, seeded admin login, JWT auth, upload endpoint, authenticated SignalR messaging, presence, typing, read receipts, sender recall, and admin delete have been verified locally.
