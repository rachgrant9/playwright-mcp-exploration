# TODO Application

A simple .NET 10 TODO application with a RESTful API and web UI. This is an MVP built following TDD principles, KISS approach, and SOLID design.

## What It Does

This application provides a basic TODO list manager with:
- RESTful API for managing todos (Create, Read, Update, Delete)
- In-memory SQLite database
- Simple web UI for interacting with todos
- Comprehensive test coverage using xUnit

## Running Locally

### Prerequisites
- .NET 10 SDK

### Start the Application

```bash
cd src/PlaywrightMcpExploration.Web
dotnet run
```

The application will start on `http://localhost:5098` (or check console output for the actual port).

Visit `http://localhost:5098` in your browser to access the web UI.

## API Endpoints

All endpoints are prefixed with `/api/todos`.

### GET /api/todos
Get all todos.

**Response:** `200 OK` with array of todos

### GET /api/todos/{id}
Get a specific todo by ID.

**Response:** 
- `200 OK` with todo object
- `404 Not Found` if todo doesn't exist

### POST /api/todos
Create a new todo.

**Request Body:**
```json
{
  "title": "Your todo title",
  "isCompleted": false
}
```

**Validation:**
- `title`: Required, 1-200 characters

**Response:**
- `201 Created` with created todo and location header
- `400 Bad Request` for validation errors

### PUT /api/todos/{id}
Update an existing todo.

**Request Body:**
```json
{
  "title": "Updated title",
  "isCompleted": true
}
```

**Validation:**
- `title`: Required, 1-200 characters

**Response:**
- `200 OK` with updated todo
- `404 Not Found` if todo doesn't exist
- `400 Bad Request` for validation errors

### DELETE /api/todos/{id}
Delete a todo.

**Response:**
- `204 No Content` on successful deletion
- `404 Not Found` if todo doesn't exist

## Running Tests

Run all tests from the repository root:

```bash
dotnet test
```

Run tests with detailed output:

```bash
dotnet test --logger "console;verbosity=detailed"
```

Run tests in a specific project:

```bash
dotnet test tests/PlaywrightMcpExploration.Tests
```

## Project Structure

- `src/PlaywrightMcpExploration.Web/` - Web application and API
- `tests/PlaywrightMcpExploration.Tests/` - xUnit test project

## Development Standards

This project follows:
- **KISS** - Keep It Simple, prioritizing MVP approach
- **TDD** - Test-Driven Development (Red-Green-Refactor)
- **SOLID** principles
- .NET 10 best practices
