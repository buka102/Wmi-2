# WMI Development Exercise

This project was created as an exercise for an interview. While some features included are not strictly necessary for a small project, they reflect good enterprise-level practices such as FluentValidation, custom exception handling, and structured logging.

## Getting Started

### Prerequisites

- [.NET 8 SDK](https://dotnet.microsoft.com/)
- [Docker](https://www.docker.com/) (if using containerized deployment)
- [Postman](https://www.postman.com/) (optional, for API testing)

### Running the Project

**Clone the repository** (if applicable):
   ```sh
   git clone https://github.com/buka102/Wmi-2
   cd Wmi-2
   cd wmi
   ```

#### Run with Docker
* with Docker: Run Api and Db (Postman collection will work as is)
```
    docker-compose up
```
Web Api runs on port 8081

#### Run without Docker

* with .NET CLI (if using Postman - need to change port value for server_host variable)
``` 
    cd src/Wmi.Api
    dotnet restore
    dotnet build
    dotnet run
```

Project should run on port 5085 (you may need to change server_host to point to this port in Postman collection)

You also need to make sure you run only `db` container in `docker-compose.yaml`

*Running DB Only*
```
docker-compose up db
```

To remove external volume, and reconstruct DB:
```
docker-compose down -v 
```



## API Testing

Use the included postman_collection.json to test API endpoints with Postman


## Enterprise-Level Features Used
This project includes several best practices commonly used in enterprise applications:

* FluentValidation: Provides a structured and testable way to validate input models (see `Wmi/src/Wmi.Api/Models/Buyer.cs` and `Wmi/src/Wmi.Api/Models/Product.cs`) and also provided as Dependency Injection to services.
* Custom Exception Handling: Centralized error handling improves security and maintainability.
* Logging (Serilog): Ensures structured logging for better debugging and monitoring.
* Dependency Injection: Promotes modularity and testability.
* Docker Support: Simplifies deployment and environment consistency.
* AutoMapper: a better way to copy values from DTOs to Objects
* Dapper with Postgre: a lightweight ORM
* Swagger: API definition exposure for operability 

### Disclaimer

While these features are overkill for a small project, they showcase best practices for large-scale applications.

## Potential Improvements for Enterprise Readiness
To make this solution more secure, maintainable, and scalable:

**Security Enhancements**

* Implement authentication and authorization (e.g., Asymetrical JWT, OAuth2).
* Secure API endpoints with role-based access control.
* Validate and sanitize all user inputs to prevent security vulnerabilities.

**Maintainability and Scalability**

* Use API versioning for backward compatibility.
* Separate configuration settings into environment variables for better flexibility.

**Testability and Observability**

* Expand unit and integration test coverage.
* Add distributed tracing (e.g., OpenTelemetry) for better debugging.
* Implement structured logging with correlation IDs.

**Performance**
* Imlement caching (Redis)

... and much more... after you hire me.
