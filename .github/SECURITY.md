# Security Policy

## Supported Versions

| Version | Supported          |
| ------- | ------------------ |
| 0.1.x   | :white_check_mark: |

## Reporting a Vulnerability

We take the security of StayHub seriously. If you discover a security vulnerability, please follow these steps:

### 1. Do Not Open a Public Issue

Please **do not** open a GitHub issue for security vulnerabilities, as this could expose the vulnerability to malicious actors.

### 2. Contact Us Privately

Send a detailed report to the maintainer via:

- **GitHub:** Open a private security advisory via the "Security" tab of this repository
- **Email:** Contact the repository owner directly

### 3. Include the Following Information

- A description of the vulnerability
- Steps to reproduce the issue
- Potential impact of the vulnerability
- Any suggested fixes (optional but appreciated)

### 4. Response Timeline

- **Initial Response:** Within 48 hours
- **Status Update:** Within 7 days
- **Resolution:** Depends on complexity, typically within 30 days

## Security Best Practices for Contributors

When contributing to StayHub, please ensure:

1. **Never commit sensitive data** (passwords, API keys, tokens, connection strings) to the repository
2. **Validate all user input** before processing to prevent injection attacks
3. **Use parameterised SQL** for all database queries (no string concatenation)
4. **Use exception handling** for all I/O and database operations
5. **Do not hardcode credentials** or configuration values in source code
6. **Hash all passwords** using BCrypt before storage
7. **Handle thread safety** carefully when working with shared resources

## Known Security Features

StayHub implements the following security measures:

- :white_check_mark: BCrypt password hashing for all user credentials
- :white_check_mark: Parameterised SQL queries preventing SQL injection
- :white_check_mark: Input validation for all user-facing fields
- :white_check_mark: Role-based access control preventing unauthorized actions
- :white_check_mark: Connection string stored in configuration, not source code
- :white_check_mark: Graceful error handling for invalid input types

## Acknowledgments

We appreciate security researchers who responsibly disclose vulnerabilities. Contributors who report valid security issues will be acknowledged in our release notes (with permission).
