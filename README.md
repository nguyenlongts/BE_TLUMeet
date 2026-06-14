# TLUMeet — Online Meeting System (Microservices)

## Architecture Overview

```
Client → API Gateway (Ocelot :5555) → Services → Kafka → Notification Service
                         │
                         └── SignalR hub (/hubs/notification) for real-time push
```

### Services

| Service              | Port | Database               | Description                                                                   |
| -------------------- | ---- | ---------------------- | ----------------------------------------------------------------------------- |
| API Gateway          | 5555 | —                      | Ocelot reverse proxy, JWT auth, WebSocket passthrough                         |
| Auth Service         | 8081 | AuthDB (MSSQL)         | Register, login, Google OAuth, email verification, JWT, password reset/change |
| Meeting Service      | 8083 | MeetingDB (MSSQL)      | Create/schedule/manage meetings, invites, JaaS tokens, lifecycle events       |
| Notification Service | 8082 | NotificationDB (MSSQL) | Email + in-app/real-time (SignalR) notifications                              |
| User Service         | 3001 | MongoDB                | User profile management                                                       |
| Admin Service        | 3002 | MongoDB                | Admin dashboard & stats                                                       |

### Infrastructure

| Component  | Port               | Description                          |
| ---------- | ------------------ | ------------------------------------ |
| SQL Server | 14333              | Persistent storage for .NET services |
| MongoDB    | 27018              | Storage for Node.js services         |
| Kafka (x3) | 9092 / 9094 / 9095 | KRaft cluster (3 brokers)            |
| Kowl UI    | 8080               | Kafka topic browser                  |

---

## Key Features

- **Auth:** email/password register + login, **Google OAuth (ID-token)**, JWT access + refresh tokens, account lockout, password reset & change.
- **Email verification:** new accounts must verify their email before login (Google accounts auto-verified).
- **Meetings:** create-now / schedule, edit, delete, join by room code, host start/end, JaaS (Jitsi) JWT generation.
- **Invitations:** invite by email, accept/decline, host gets responses.
- **Real-time (SignalR):** invite & invite-response notifications, meeting started/ended lifecycle
- **Email notifications:** welcome, password reset, password-changed security alert, email verification, meeting invite.
- **Transactional outbox** on Auth & Meeting services → Kafka → Notification consumers (eventual consistency).

---

## Tech Stack

- **.NET 8** — Auth, Meeting, Notification services (Clean Architecture)
- **Node.js** — User, Admin services
- **Ocelot** — API Gateway (HTTP + WebSocket for SignalR)
- **SignalR** — Real-time notifications
- **SQL Server 2022** — Primary relational database
- **MongoDB** — Document store for user/admin services
- **Apache Kafka 7.5** (KRaft, no Zookeeper) — Async event bus
- **Google.Apis.Auth** — Google ID-token verification
- **Docker / Docker Compose** — Container orchestration
- **JWT** — Authentication & authorization

---

## Clean Architecture (per .NET service)

```
Service/
├── ServiceName.API/           # Controllers, SignalR hubs, Kafka consumers, DI config
├── ServiceName.Application/   # Use cases, DTOs, interfaces
├── ServiceName.Domain/        # Entities, domain models
└── ServiceName.Infrastructure/# EF Core, Kafka producer, outbox relay, repositories
```

---

## Kafka Topics

| Topic                       | Producer        | Consumer             | Purpose                              |
| --------------------------- | --------------- | -------------------- | ------------------------------------ |
| `user-registered-events`    | Auth Service    | Notification Service | Welcome email (after verification)   |
| `email-verification-events` | Auth Service    | Notification Service | Email-verification link              |
| `password-reset-events`     | Auth Service    | Notification Service | Password reset link                  |
| `password-changed-events`   | Auth Service    | Notification Service | Password-changed security alert      |
| `meeting-created-events`    | Meeting Service | Notification Service | —                                    |
| `meeting-started-events`    | Meeting Service | Notification Service | Notify accepted invitees + room push |
| `meeting-ended-events`      | Meeting Service | Notification Service | Real-time kick-out of participants   |
| `meeting-deleted-events`    | Meeting Service | —                    |                                      |
| `meeting-invited-events`    | Meeting Service | Notification Service | Invite bell + email                  |
| `invite-responded-events`   | Meeting Service | Notification Service | Notify host of accept/decline        |
| `participant-joined-events` | Meeting Service | —                    |                                      |
| `participant-left-events`   | Meeting Service | —                    |                                      |

Topics auto-create on first publish (`KAFKA_AUTO_CREATE_TOPICS_ENABLE=true`).

---

## Setup & Run

### Prerequisites

- Docker Desktop

### 1. Create `.env` file (at `BE_TLUMeet/`)

```env
AUTH_VERSION=1.0.0
MEETING_VERSION=1.0.0
NOTI_VERSION=1.0.0
SA_PASSWORD=YourStrong@Passw0rd
JWT_KEY=your_jwt_secret_key
```

### 2. Required service configuration

Some features need extra settings (in each service's `appsettings.json` or as env vars):

- **Auth Service** — `Google:ClientId` (Google OAuth client ID) for `/api/Auth/google`.
- **Notification Service** — `FE:BaseUrl` (e.g. `http://localhost:5173`) used to build links in emails (verify, reset, invite), plus SMTP settings for `IEmailService`.

### 3. Build & start all services

```bash
docker compose up -d --build
```

> First run takes a few minutes while Kafka brokers and SQL Server initialize. .NET services apply EF Core migrations automatically on startup (`db.Database.Migrate()`).

### 4. Stop all services

```bash
docker compose down          # keep data
docker compose down -v       # also wipe databases/volumes
```

### Rebuilding a single service

```bash
docker compose up -d --build authservice      # or meetingservice / notiservice / gateway
```

---

## API Endpoints (via Gateway — `http://localhost:5555`)

### Auth — `/api/Auth`

| Method | Path                   | Auth | Description                                        |
| ------ | ---------------------- | ---- | -------------------------------------------------- |
| POST   | `/register`            | No   | Register (sends verification email; no auto-login) |
| POST   | `/login`               | No   | Login, returns JWT + refresh token                 |
| POST   | `/google`              | No   | Login/register with Google ID token                |
| POST   | `/verify-email`        | No   | Verify email via token                             |
| POST   | `/resend-verification` | No   | Resend verification email                          |
| POST   | `/forgot-password`     | No   | Send password reset link                           |
| POST   | `/reset-password`      | No   | Reset password via token                           |
| POST   | `/change-password`     | Yes  | Change current password                            |
| GET    | `/me`                  | Yes  | Current user claims                                |
| POST   | `/refresh`             | No   | Refresh access token                               |
| POST   | `/logout`              | No   | Revoke refresh token                               |

### Meetings — `/api/meeting`

| Method | Path                         | Auth | Description                 |
| ------ | ---------------------------- | ---- | --------------------------- |
| POST   | `/`                          | Yes  | Create / schedule a meeting |
| GET    | `/`                          | No   | Get all meetings            |
| GET    | `/{id}`                      | No   | Get meeting by id           |
| GET    | `/host/{hostEmail}`          | No   | Meetings hosted by a user   |
| GET    | `/invited`                   | Yes  | Meetings the user accepted  |
| PUT    | `/`                          | No   | Update meeting              |
| DELETE | `/{id}`                      | Yes  | Delete meeting (host/admin) |
| GET    | `/check/{roomCode}`          | No   | Check room code exists      |
| GET    | `/{roomCode}/status`         | No   | Meeting status              |
| POST   | `/{roomCode}/start`          | Yes  | Start meeting (host)        |
| POST   | `/{roomCode}/end`            | Yes  | End meeting (host)          |
| POST   | `/{roomCode}/join`           | No   | Join a meeting              |
| POST   | `/leave`                     | No   | Leave a meeting             |
| GET    | `/{roomCode}/participants`   | No   | List participants           |
| POST   | `/{roomCode}/invite`         | Yes  | Invite people by email      |
| POST   | `/invite/{inviteId}/respond` | Yes  | Accept / decline an invite  |

### JaaS (Jitsi) — `/api/jaas`

| Method | Path              | Auth | Description                  |
| ------ | ----------------- | ---- | ---------------------------- |
| POST   | `/generate-token` | —    | Generate JaaS JWT for a room |

### Notifications — `/api/notification`

| Method | Path         | Auth | Description       |
| ------ | ------------ | ---- | ----------------- |
| GET    | `/`          | Yes  | Get notifications |
| PUT    | `/read-all`  | Yes  | Mark all as read  |
| PUT    | `/{id}/read` | Yes  | Mark one as read  |

### Real-time — SignalR

- Hub: `ws(s)://localhost:5555/hubs/notification` (JWT via `access_token` query; anonymous allowed for room groups).
- Client events: `ReceiveInvite`, `ReceiveInviteResponse`, `MeetingStarted`, `MeetingEnded`.
- Hub methods: `JoinMeetingGroup(roomCode)`, `LeaveMeetingGroup(roomCode)`.

---

## Monitoring

- **Kafka UI (Kowl):** http://localhost:8080 — browse topics, consumer groups, messages.
  </content>
