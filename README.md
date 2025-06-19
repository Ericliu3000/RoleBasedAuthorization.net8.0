
ASP.NET RBAC Permission Management

Instructions:
This example extends Microsoft's ContosoUniversity program to implement resource-based permission management for all pages using Sang.AspNetCore.RoleBasedAuthorization.
Key Features:
Resource Assignment: Resources can be authorized to roles or directly to users.
User & Role Management: Added functionalities for managing users and roles.
Implementation Highlights:
Leverages the Sang.AspNetCore.RoleBasedAuthorization library to enable fine-grained control over page access.
Resources (e.g., pages, API endpoints) are mapped to roles or individual users through a configurable permission system.
Provides a unified interface for assigning, revoking, and verifying permissions, ensuring secure access based on role membership or direct user grants.
Technical Architecture:
Builds on ContosoUniversity's existing data model and UI.
Integrates role-based authorization via dependency injection and custom authorization handlers.
Extends Identity framework to support role-resource and user-resource mappings in the database.

  https://github.com/Ericliu3000/RoleBasedAuthorization.net8.0
