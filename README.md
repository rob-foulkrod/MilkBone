# MilkBone

A full-featured **Todo application** built with **.NET 10 MVC**, an in-memory repository, and a complete xUnit test suite.

---

## Features

- **Create, read, update, and delete** todo items
- **Priority levels** — Low, Medium, High, Critical — with colour-coded visual cues
- **Due-date tracking** — items flagged overdue or due today
- **Toggle completion** — mark items done or reopen them with one click
- **Filter views** — All / Active / Completed tab pills on the index page
- **Stats dashboard** — live counts for Total, Active, Completed, Overdue, and Due Today
- **Orange + teal + slate UI theme** with stat cards and priority-striped todo cards

---

## Architecture

```
MilkBone/
├── TodoApp/                     # ASP.NET Core MVC web application
│   ├── Controllers/
│   │   └── TodoController.cs    # Index, Create, Edit, Delete, Details, Toggle
│   ├── Models/
│   │   ├── TodoItem.cs          # Core entity with validation attributes
│   │   └── TodoPriority.cs      # Low / Medium / High / Critical enum
│   ├── Repositories/
│   │   ├── ITodoRepository.cs   # Repository abstraction
│   │   └── InMemoryTodoRepository.cs  # Thread-safe in-memory store
│   ├── Services/
│   │   ├── ITodoService.cs      # Service abstraction
│   │   ├── TodoService.cs       # Business logic: CRUD, toggle, stats, validation
│   │   └── TodoStats.cs         # Aggregate stats record
│   ├── Views/Todo/              # Razor views for each action
│   └── wwwroot/                 # Static assets (custom site.css)
└── TodoApp.Tests/               # xUnit test project
    ├── InMemoryTodoRepositoryTests.cs
    └── TodoServiceTests.cs
```

### Data Model — `TodoItem`

| Property | Type | Notes |
|---|---|---|
| `Id` | `int` | Auto-assigned by repository |
| `Title` | `string` | Required; 1–200 characters |
| `Description` | `string?` | Optional; max 1 000 characters |
| `IsCompleted` | `bool` | Toggled via the Toggle endpoint |
| `Priority` | `TodoPriority` | Default: Medium |
| `DueDate` | `DateTime?` | Optional |
| `CreatedAt` | `DateTime` | Set on creation (UTC) |
| `CompletedAt` | `DateTime?` | Set when item is toggled complete |

### Repository layer

`ITodoRepository` defines the CRUD contract. `InMemoryTodoRepository` fulfils it using a thread-safe in-memory dictionary that stores deep clones of each item to mirror EF Core change-tracking semantics — making a future swap to a database provider straightforward.

### Service layer

`TodoService` owns all business logic:

- Input validation (title length, non-empty)
- CRUD operations delegated to the repository
- `ToggleComplete` — sets or clears `CompletedAt` and `IsCompleted`
- `GetStats` — computes Total / Active / Completed / Overdue / DueToday in a single pass

---

## Getting Started

### Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0)

### Run the application

```bash
dotnet run --project TodoApp
```

The app starts at `https://localhost:5001` (or the port shown in the console) and defaults to the Todo index page.

### Run the tests

```bash
dotnet test
```

```
Passed! - Failed: 0, Passed: 40, Skipped: 0, Total: 40
```

---

## Endpoints

| Method | Route | Description |
|---|---|---|
| GET | `/Todo` | Index — list all todos (supports `?filter=active\|completed`) |
| GET | `/Todo/Details/{id}` | View a single todo |
| GET | `/Todo/Create` | Create form |
| POST | `/Todo/Create` | Submit new todo |
| GET | `/Todo/Edit/{id}` | Edit form |
| POST | `/Todo/Edit/{id}` | Submit edits |
| GET | `/Todo/Delete/{id}` | Confirm delete |
| POST | `/Todo/Delete/{id}` | Confirm and delete |
| POST | `/Todo/Toggle/{id}` | Toggle completion status |

---

## Tests

40 xUnit tests across two test classes:

| Class | Coverage |
|---|---|
| `InMemoryTodoRepositoryTests` | CRUD operations, ID assignment, clone isolation |
| `TodoServiceTests` | Validation, CRUD, toggle idempotency, stats (overdue, due-today, completed exclusions) |

Closes #7