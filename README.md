# TLUMeet - Online Meeting System (Microservices)

---

Architecture:

* API Gateway: Ocelot
* Services:

  * Auth Service
  * Meeting Service
  * Notification Service
* Database: SQL Server (each service has its own database)
* Message Broker: Kafka (for asynchronous communication between services)

### Flow Overview

Client → API Gateway → Services → Kafka → Notification Service

### Service Architecture

Each service follows Clean Architecture:

- Domain Layer  
- Application Layer  
- Infrastructure Layer  
- API Layer  

---

## Setup & Run

### 1. Start services (Services + Kafka)

```bash
docker-compose up --build
```


## Tech Stack

* .NET 8
* ASP.NET Core Web API
* Ocelot API Gateway
* SQL Server
* Docker
* Kafka

