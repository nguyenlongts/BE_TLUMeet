# TLUMeet — Online Meeting System (Microservices)

## Architecture Overview

```
Client → API Gateway (Ocelot :5555) → Services → Kafka → Notification Service
```

### Services

| Service              | Port  | Database         | Description                        |
|----------------------|-------|------------------|------------------------------------|
| API Gateway          | 5555  | —                | Ocelot reverse proxy & JWT auth    |
| Auth Service         | 8081  | AuthDB (MSSQL)   | Register, login, JWT, reset password |
| Meeting Service      | 8083  | MeetingDB (MSSQL)| Create, schedule, manage meetings  |
| Notification Service | 8082  | NotificationDB (MSSQL) | Email & in-app notifications  |
| User Service         | 3001  | MongoDB          | User profile management            |
| Admin Service        | 3002  | MongoDB          | Admin dashboard & stats            |

### Infrastructure

| Component   | Port  | Description                        |
|-------------|-------|------------------------------------|
| SQL Server  | 14333 | Persistent storage for .NET services |
| MongoDB     | 27018 | Storage for Node.js services       |
| Kafka (x3)  | 9092 / 9094 / 9095 | KRaft cluster (3 brokers) |
| Kowl UI     | 8080  | Kafka topic browser                |

---

## Tech Stack

- **.NET 8** — Auth, Meeting, Notification services (Clean Architecture)
- **Node.js** — User, Admin services
- **Ocelot** — API Gateway
- **SQL Server 2022** — Primary relational database
- **MongoDB** — Document store for user/admin services
- **Apache Kafka 7.5** (KRaft, no Zookeeper) — Async event bus
- **Docker / Docker Compose** — Container orchestration
- **JWT** — Authentication & authorization

---

## Clean Architecture (per .NET service)

```
Service/
├── ServiceName.API/           # Controllers, middleware, DI config
├── ServiceName.Application/   # Use cases, DTOs, interfaces
├── ServiceName.Domain/        # Entities, domain models
└── ServiceName.Infrastructure/# EF Core, Kafka producer/consumer, repositories
```

---

## Kafka Topics

| Topic                      | Producer         | Consumer             |
|----------------------------|------------------|----------------------|
| `user-registered-events`   | Auth Service     | Notification Service |
| `password-reset-events`    | Auth Service     | Notification Service |
| `user-updated-events`      | User Service     | —                    |
| `meeting-created-events`   | Meeting Service  | Notification Service |
| `meeting-started-events`   | Meeting Service  | Notification Service |
| `meeting-ended-events`     | Meeting Service  | Notification Service |
| `meeting-deleted-events`   | Meeting Service  | Notification Service |
| `meeting-invited-events`   | Meeting Service  | Notification Service |
| `invite-responded-events`  | Meeting Service  | Notification Service |
| `participant-joined-events`| Meeting Service  | Notification Service |
| `participant-left-events`  | Meeting Service  | —                    |

All topics: **3 partitions, replication factor 3**.

---

## Setup & Run

### Prerequisites

- Docker Desktop

### 1. Create `.env` file

```env
JWT_KEY=your_jwt_secret_key
SA_PASSWORD=YourStrong@Passw0rd
AUTH_VERSION=1.0.0
MEETING_VERSION=1.0.0
NOTI_VERSION=1.0.0
```

### 2. Build & start all services

```bash
docker compose up -d --build
```

> First run takes a few minutes while Kafka brokers and SQL Server initialize.

### 3. Create Kafka topics (first run only)

```bash
docker exec kafka-1 kafka-topics --bootstrap-server localhost:9092 \
  --create --topic user-registered-events --partitions 3 --replication-factor 3

# Repeat for each topic listed above, or use KAFKA_AUTO_CREATE_TOPICS_ENABLE=true (already set)
```

### 4. Stop all services

```bash
docker compose down
```

> To also remove volumes (wipes databases): `docker compose down -v`

---

## API Endpoints (via Gateway — `http://localhost:5555`)

### Auth
| Method | Path                        | Auth | Description              |
|--------|-----------------------------|------|--------------------------|
| POST   | `/api/Auth/register`        | No   | Register new user        |
| POST   | `/api/Auth/login`           | No   | Login, returns JWT       |
| POST   | `/api/Auth/logout`          | Yes  | Revoke refresh token     |
| POST   | `/api/Auth/refresh`         | No   | Refresh access token     |
| POST   | `/api/Auth/forgot-password` | No   | Send password reset link |
| POST   | `/api/Auth/reset-password`  | No   | Reset password via token |
| POST   | `/api/Auth/change-password` | Yes  | Change current password  |

### Meetings
| Method | Path                        | Auth | Description              |
|--------|-----------------------------|------|--------------------------|
| GET    | `/api/Meeting`              | Yes  | Get meetings by email    |
| POST   | `/api/Meeting/schedule`     | Yes  | Schedule a new meeting   |
| PUT    | `/api/Meeting/{id}`         | Yes  | Update meeting           |
| DELETE | `/api/Meeting/{id}`         | Yes  | Delete meeting           |
| POST   | `/api/Meeting/join`         | Yes  | Join a meeting           |
| GET    | `/api/Meeting/check/{code}` | Yes  | Check room code          |
| POST   | `/api/Meeting/start`        | Yes  | Start meeting now        |

### Notifications
| Method | Path                          | Auth | Description              |
|--------|-------------------------------|------|--------------------------|
| GET    | `/api/Notification`           | Yes  | Get notifications        |
| PUT    | `/api/Notification/{id}/read` | Yes  | Mark as read             |

---

## Monitoring

- **Kafka UI (Kowl):** http://localhost:8080 — browse topics, consumer groups, messages
