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
   git clone <repository-url>
   cd wmi-2
   ```

**Provision DB**
```
cd db
docker-compose up
```

To remove external volume, and reconstruct DB:
```
docker-compose down -v 
```

**Run**
* with .NET CLI (if using Postman - need to change port value for server_host variable)
``` 
    dotnet build
    dotnet run --project Wmi.Api/Wmi.Api.csproj
```
Or 
* with Docker (Postman collection will work as is)
```
    docker-compose up
```

**API Testing**

Use the included postman_collection.json to test API endpoints with Postman


## Enterprise-Level Features Used
### This project includes several best practices commonly used in enterprise applications:

* FluentValidation: Provides a structured and testable way to validate input models.
* Custom Exception Handling: Centralized error handling improves security and maintainability.
* Logging (Serilog): Ensures structured logging for better debugging and monitoring.
* Dependency Injection: Promotes modularity and testability.
* Docker Support: Simplifies deployment and environment consistency.

While these features are overkill for a small project, they showcase best practices for large-scale applications.

## Potential Improvements for Enterprise Readiness
### To make this solution more secure, maintainable, and scalable:

**Security Enhancements**

* Implement authentication and authorization (e.g., JWT, OAuth2).
* Secure API endpoints with role-based access control.
* Validate and sanitize all user inputs to prevent security vulnerabilities.

**Maintainability and Scalability**

* Use API versioning for backward compatibility.
* Separate configuration settings into environment variables for better flexibility.

**Testability and Observability**

* Expand unit and integration test coverage.
* Add distributed tracing (e.g., OpenTelemetry) for better debugging.
* Implement structured logging with correlation IDs.



