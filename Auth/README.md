# AuthService Readme

## Overview

The **AuthService** provides authentication, user registration, and user session management functionalities. It offers endpoints for login, logout, user registration, and obtaining user information. It uses JWT-based authentication and sets a secure cookie for session management.

## Development Setup
### Prerequisites
  .NET SDK 8.0 or later
  Database with connection string configured in appsettings.json or as environment variables.
  Docker (optional for containerization).
### Running Locally
  Clone the repository.
  Configure the appsettings.json file with the required values (e.g., Database:ConnectionString, JWTKey, etc.).
  Run the application:
```bash
dotnet run
```

### Running with Docker
  Build the Docker image:

```bash
docker-compose build
```

### Run the container:
```bash
docker-compose run
```
 
## Authentication Flow
1. Login:
  - User provides credentials.
  - Receives an encrypted JWT token in an HTTP-only cookie.
2. Session Validation:
  - On subsequent requests, the cookie is used to validate the user's session.
3. Protected Resources:
- Use the Authorization attribute to secure endpoints.

## Security Notes
- Use HTTPS to ensure secure transmission of cookies and data.
- Configure JWTKey:Secret with a strong and unique value.
- Rotate the JWTKey:Secret periodically for enhanced security.