

# StockSync

StockSync is a microservices-based inventory management system built with ASP.NET Core Web API. It provides scalable, containerized services for managing suppliers and inventory items, using modern technologies and best practices.

## Architecture

- **Supplier Service**: Handles supplier management, authentication, and authorization. Uses MS SQL Server and Entity Framework Core. Supports syncing supplier items from the Item Service and uses Hangfire for background jobs.
- **Item Service**: Manages inventory items. Uses MongoDB for storage. Provides RESTful endpoints for CRUD operations and batch queries.
- **Shared Library**: Contains shared models, user roles, and middleware for logging and global exception handling.
- **Docker Compose**: Orchestrates all services and databases for local development and deployment.

## Features

- Microservices architecture
- RESTful APIs for suppliers and items
- JWT-based authentication and role-based authorization
- Centralized exception handling and request/response logging (Serilog)
- Supplier-Item sync with caching (in-memory)
- Hangfire background jobs for scheduled sync
- Dockerized services and databases (SQL Server, MongoDB)

## Recent Changes

- Supplier Service now syncs item details from Item Service and caches them for fast access
- Hangfire integration for scheduled supplier sync jobs
- Improved error handling and logging
- Entity Framework Core and MongoDB EF Core integration
- New endpoints for batch item queries and supplier-item relationships

## Getting Started

### Prerequisites

- Docker & Docker Compose
- .NET 9 SDK (for local development)

### Setup & Run

1. Clone the repository:
	```sh
	git clone https://github.com/MadurangaNamal/StockSync.git
	cd StockSync
	```
2. Create a `.env` file in the root directory with the following content:
	```env
	DB_PASSWORD=YourStrongPassword
	JWT_SECRET_KEY=YourJWTSecretKey
	```
3. Start all services using Docker Compose:
	```sh
	docker-compose up --build
	```
4. Access APIs:
	- Supplier Service: `http://localhost:5001`
	- Item Service: `http://localhost:5002`

## API Documentation

You can use the included Postman collection (`StockSync.postman_collection.json`) for testing endpoints.

## Project Structure

- `StockSync.SupplierService/` - Supplier microservice
- `StockSync.ItemService/` - Item microservice
- `StockSync.Shared/` - Shared code
- `docker-compose.yml` - Service orchestration

## Background Jobs

- Supplier Service uses Hangfire to schedule supplier-item sync every 10 minutes and on startup.
- Hangfire dashboard available at `/hangfire` (when running in development).

## Contributing

Pull requests are welcome. For major changes, please open an issue first to discuss what you would like to change.
