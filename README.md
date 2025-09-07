
# StockSync

StockSync is a microservices-based inventory management system built with ASP.NET Core Web API. It provides scalable, containerized services for managing suppliers and inventory items, using modern technologies and best practices.

## Architecture

- **Supplier Service**: Handles supplier management, authentication, and authorization. Uses MS SQL Server and Entity Framework Core.
- **Item Service**: Manages inventory items. Uses MongoDB for storage.
- **Shared Library**: Contains shared models and middleware.
- **Docker Compose**: Orchestrates all services and databases for local development and deployment.

## Features

- Microservices architecture
- RESTful APIs
- JWT-based authentication
- Centralized exception handling
- Logging with Serilog
- Dockerized services and databases

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

## Contributing

Pull requests are welcome. For major changes, please open an issue first to discuss what you would like to change.
