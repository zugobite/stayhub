# Contributing to StayHub

First off, thank you for considering contributing to StayHub! Your help makes this project better for everyone.

## Table of Contents

- [Code of Conduct](#code-of-conduct)
- [Getting Started](#getting-started)
- [How Can I Contribute?](#how-can-i-contribute)
- [Development Setup](#development-setup)
- [Style Guidelines](#style-guidelines)
- [Commit Messages](#commit-messages)
- [Pull Request Process](#pull-request-process)

---

## Code of Conduct

This project and everyone participating in it is governed by our commitment to providing a welcoming and inclusive environment. By participating, you are expected to uphold this standard. Please report unacceptable behavior to the repository maintainers.

---

## Getting Started

Before you begin:

- Make sure you have a [GitHub account](https://github.com/signup)
- Ensure you have the [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0) installed (`dotnet --version`)
- Ensure you have access to a [SQL Server](https://www.microsoft.com/en-us/sql-server/) instance (Docker, LocalDB, Express, or full)
- Install the EF Core tools if you intend to change the schema (`dotnet tool install --global dotnet-ef --version 8.0.24`)
- Familiarize yourself with the [project structure](README.md#project-structure)
- Check existing [issues](../../issues) to see if your contribution is already being discussed

---

## How Can I Contribute?

### Reporting Bugs

Before creating bug reports, please check existing issues. When filing a bug report, use our [bug report template](.github/ISSUE_TEMPLATE/bug_report.md) and include:

- A clear and descriptive title
- Steps to reproduce the behavior
- Expected vs actual behavior
- Your environment (OS, .NET version, SQL Server version)

### Suggesting Enhancements

Enhancement suggestions are tracked as GitHub issues. Use our [feature request template](.github/ISSUE_TEMPLATE/feature_request.md) and provide:

- A clear and descriptive title
- A detailed description of the proposed enhancement
- Why this enhancement would be useful
- Any alternatives you've considered

### Pull Requests

1. Fork the repository
2. Create a new branch from `main`
3. Make your changes
4. Submit a pull request

---

## Development Setup

### Prerequisites

- .NET 8 SDK or higher
- SQL Server (Docker, LocalDB, Express, or full instance)
- A modern browser
- Git

### Local Setup

```bash
# 1. Clone your fork
git clone https://github.com/YOUR_USERNAME/stayhub.git
cd stayhub

# 2. Start SQL Server and create the database
docker compose up -d

# 3. Update the connection string if you are not using Docker
# Edit src/StayHub.Web/appsettings.json

# 4. Build the solution
dotnet build

# 5. Run the application (migrations and seeding run automatically)
dotnet run --project src/StayHub.Web
```

### Project Structure

```
src/
├── StayHub.Core/          # Domain models, enums, role constants, interfaces
├── StayHub.Data/          # EF Core context, configurations, migrations, repositories, seeders
└── StayHub.Web/           # ASP.NET Core MVC controllers, views, view models, services
```

---

## Style Guidelines

### C# Code Style

- Use **4 spaces** for indentation (no tabs)
- Use **meaningful variable and method names** in camelCase for locals, PascalCase for public members
- Use **PascalCase** for class names, methods, and properties
- Use **UPPER_SNAKE_CASE** or PascalCase for constants
- Add **XML documentation comments** (`///`) for all public classes, methods, and properties
- Keep methods short and focused - one responsibility per method
- Prefer **early returns** over deep nesting
- Use **expression-bodied members** where appropriate

### Example

```csharp
/// <summary>
/// Validates that the given string is not null or whitespace.
/// </summary>
/// <param name="input">The string to validate.</param>
/// <returns>True if the string is non-empty, false otherwise.</returns>
public static bool IsNonEmpty(string input)
{
    return !string.IsNullOrWhiteSpace(input);
}
```

### Package/Namespace Conventions

- `StayHub.Core.Models` - Domain entity classes
- `StayHub.Core.Enums` - Enumeration types
- `StayHub.Core.Interfaces` - Repository and service contracts
- `StayHub.Core.Constants` - Role names and other compile-time constants
- `StayHub.Data.Configurations` - EF Core fluent entity configurations
- `StayHub.Data.Repositories` - EF Core repository implementations
- `StayHub.Data.Seeders` - Startup role, SuperAdmin, and demonstration data seeding
- `StayHub.Web.Controllers` - MVC controllers
- `StayHub.Web.ViewModels` - Form and dashboard view models
- `StayHub.Web.Services` - File storage and role transition services

### Security Guidelines

- **Never commit sensitive data** (passwords, API keys, tokens) to the repository
- **Validate all user input** before processing to prevent injection attacks
- **Use LINQ or parameterised SQL** for all database access (never string concatenation)
- **Use exception handling** for all I/O and database operations
- **Do not hardcode credentials** or configuration values in source code
- **Leave password handling to ASP.NET Core Identity** - never hash or compare credentials by hand
- **Protect every state-changing POST** with `[ValidateAntiForgeryToken]`
- **Re-check ownership in the data layer**, not only in the route (a user must not act on another user's records)

---

## Commit Messages

Follow the [Conventional Commits](https://www.conventionalcommits.org/) specification:

```
<type>(<scope>): <description>

[optional body]

[optional footer]
```

### Types

| Type       | Description                                            |
| ---------- | ------------------------------------------------------ |
| `feat`     | A new feature                                          |
| `fix`      | A bug fix                                              |
| `docs`     | Documentation changes only                             |
| `style`    | Formatting, missing semicolons, etc. (no code change)  |
| `refactor` | Code change that neither fixes a bug nor adds a feature|
| `perf`     | Performance improvement                                |
| `test`     | Adding or updating tests                               |
| `chore`    | Build process, tooling, or auxiliary changes            |

### Examples

```
feat(models): add HostProperty class with pricing and activation
fix(booking): reject overlapping dates on the same listing
docs(readme): update installation instructions
refactor(controllers): extract the role swap into RoleTransitionService
chore(gitignore): exclude runtime upload folder
```

---

## Pull Request Process

1. **Update documentation** if you're changing functionality
2. **Update the CHANGELOG.md** with your changes under `[Unreleased]`
3. **Ensure your code compiles** without errors (`dotnet build`)
4. **Fill out the PR template** completely
5. **Request a review** from a maintainer
6. **Address feedback** promptly

### PR Checklist

- [ ] Code follows the project's [style guidelines](CONTRIBUTING.md#style-guidelines)
- [ ] Self-review completed
- [ ] XML documentation comments added for public classes and methods
- [ ] Documentation updated (if applicable)
- [ ] No new compiler warnings generated
- [ ] CHANGELOG.md updated under `[Unreleased]`
- [ ] Changes compile and run successfully (`dotnet build && dotnet run --project src/StayHub.Web`)

---

## Questions?

Feel free to open an issue with the `question` label if you need help getting started.
