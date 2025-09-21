# E-Commerce Inventory API

A RESTful API for managing an e-commerce inventory system, built with .NET Core 8, Entity Framework Core, and PostgreSQL. Implements secure CRUD operations for products and categories with JWT-based authentication, including refresh tokens. Follows Domain-Driven Design (DDD) with repository and unit of work patterns, adhering to SOLID principles.

## Table of Contents

- [Tech Stack](#tech-stack)
- [Features](#features)
- [Setup Instructions](#setup-instructions)
- [API Documentation](#api-documentation)
- [Project Structure](#project-structure)
- [Running the API](#running-the-api)
- [Testing](#testing)
- [Git Commit History](#git-commit-history)
- [Bonus Features](#bonus-features)

## Tech Stack

- **Backend**: .NET Core 8
- **Database**: PostgreSQL
- **ORM**: Entity Framework Core
- **Authentication**: JWT with refresh tokens
- **API Documentation**: Swagger (Swashbuckle.AspNetCore)
- **Architecture**: Domain-Driven Design (DDD), Repository, Unit of Work patterns

## Features

### User Authentication

- **POST /api/auth/register**: Create a user with username, email, and hashed password.
- **POST /api/auth/login**: Authenticate user and return JWT access and refresh tokens.
- **POST /api/auth/refresh**: Refresh access token using refresh token.
- All endpoints are protected with JWT.

### Product Management

- **POST /api/products**: Create a product (name, description, price, stock, category ID and image).
- **GET /api/products**: List products with filters:
  - By category (`?categoryId=95b106e7-368c-439d-9014-b6eab57b25b1`)
  - By price range (`?minPrice=10&maxPrice=100`)
  - Pagination (`?page=1&limit=10`)
- **GET /api/products/{id}**: Get product by ID.
- **PUT /api/products/{id}**: Update product details.
- **DELETE /api/products/{id}**: Delete product, returns 404 if not found.
- **GET /api/products/search?q=keyword**: Search products by name or description.

### Category Management

- **POST /api/categories**: Create a category (unique name, description).
- **GET /api/categories**: List categories with product counts.
- **GET /api/categories/{id}**: Get category by ID.
- **PUT /api/categories/{id}**: Update category details.
- **DELETE /api/categories/{id}**: Delete category, returns 409 if linked to products.

## Setup Instructions

1. **Prerequisites**:

   - .NET Core 8 SDK: [Download](https://dotnet.microsoft.com/download)
   - PostgreSQL: [Download](https://www.postgresql.org/download/)
   - Git: [Download](https://git-scm.com/downloads)
   - Postman (optional): [Download](https://www.postman.com/downloads/)

2. **Clone the Repository**:

   ```bash
   git clone https://github.com/antu41/ECommerceInventoryAPI.git
   cd ECommerceInventoryAPI
   ```

3. **Configure Database**:

   - Install PostgreSQL and create a database named `ECommerceDb`.
   - Update `API/appsettings.json` with your PostgreSQL connection string:
     ```json
     {
       "ConnectionStrings": {
         "DefaultConnection": "Host=localhost;Database=ECommerceDb;Username=postgres;Password=yourpassword"
       },
       "Jwt": {
         "Key": "YourSuperSecretKeyMustBeAtLeast32CharactersLong",
         "Issuer": "ECommerceAPI",
         "Audience": "ECommerceAPI",
         "AccessTokenExpiryMinutes": "15",
         "RefreshTokenExpiryDays": "7"
       }
     }
     ```

4. **Install Dependencies**:

   - Ensure all NuGet packages are restored:
     ```bash
     dotnet restore
     ```

5. **Apply Migrations**:

   - Run Entity Framework Core migrations to create the database schema:
     ```bash
     dotnet ef database update --project Infrastructure --startup-project API
     ```

6. **Run the API**:
   - Start the API:
     ```bash
     dotnet run --project API
     ```
   - The API will be available at `https://localhost:7073`.
   - Swagger UI: `https://localhost:7073/swagger`.

## API Documentation

Swagger UI provides interactive API documentation:

- Access at `https://localhost:7073/swagger`.
- Endpoints are tagged as `Auth`, `Products`, and `Categories`.
- Authorization: Use the "Authorize" button to enter `Bearer {accessToken}` obtained from `POST /api/auth/login`.
- Example request for `POST /api/categories`:
  ```json
  {
    "name": "Electronics",
    "description": "Gadgets and devices"
  }
  ```
  - **Response (201 Created)**:
    ```json
    {
      "id": "95b106e7-368c-439d-9014-b6eab57b25b1",
      "name": "Electronics",
      "description": "Gadgets and devices",
      "productCount": 0
    }
    ```
  - **Errors**:
    - 403 Forbidden: Invalid or missing token.
    - 409 Conflict: Category name already exists.

## Project Structure

- **API/**: Controllers, middleware, and configuration (`Program.cs`, `appsettings.json`).
- **Application/**: DTOs, services, and interfaces for business logic.
- **Domain/**: Entities and repository interfaces.
- **Infrastructure/**: Entity Framework Core context and repository implementations.

## Running the API

1. Ensure PostgreSQL is running.
2. Run `dotnet run --project API` to start the API.
3. Access Swagger UI at `https://localhost:7073/swagger` or test endpoints with Postman.

## Testing

1. **Register a User**:
   ```bash
   curl -X POST "https://localhost:7073/api/auth/register" -H "Content-Type: application/json" -d '{"username":"testuser","email":"test@example.com","password":"Password123!"}'
   ```
2. **Login**:
   ```bash
   curl -X POST "https://localhost:7073/api/auth/login" -H "Content-Type: application/json" -d '{"email":"test@example.com","password":"Password123!"}'
   ```
3. **Create Category** (with JWT token):
   ```bash
   curl -X POST "https://localhost:7073/api/categories" -H "Authorization: Bearer {accessToken}" -H "Content-Type: application/json" -d '{"name":"Electronics","description":"Gadgets and devices"}'
   ```
4. **Test Filters**:
   ```bash
   curl -X GET "https://localhost:7073/api/products?categoryId=95b106e7-368c-439d-9014-b6eab57b25b1&minPrice=10&maxPrice=1000&page=1&limit=10" -H "Authorization: Bearer {accessToken}"
   ```

## Git Commit History

- Commits are frequent and descriptive, covering development stages (e.g., "Implemented JWT authentication," "Added product CRUD with filters," "Fixed Swagger JWT issue").
- View history: `git log --oneline` or on GitHub.

## Bonus Features

- **Refresh Tokens**: Implemented via `POST /api/auth/refresh`.
- **Image Uploads**: Implemented via `POST /api/products`.

## Notes

- Code adheres to SOLID principles and DDD.
- Error handling includes proper HTTP status codes (e.g., 404 for not found, 409 for conflicts, 403 for unauthorized).
- Swagger documentation includes endpoint tags and example requests/responses.

For issues, contact Email: antudas444475@gmail.com or GitHub username: antu41.
