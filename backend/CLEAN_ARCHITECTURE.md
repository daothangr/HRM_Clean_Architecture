# Backend Clean Architecture

## Current layered structure

- `HRM.API`: presentation layer (controllers, middleware, startup)
- `HRM.Application`: use-cases with MediatR commands/queries and pipeline behaviors
- `HRM.Domain`: core business entities, enums, exceptions, and domain contracts
- `HRM.Infrastructure`: EF Core persistence, identity, external implementations
- `HRM.Shared`: shared kernel (constants, helpers, extensions)

## Implemented clean architecture foundations

- Added shared kernel project: `HRM.Shared`
- Added domain abstractions:
  - `IRepository<TEntity>`
  - `IUnitOfWork`
- Added infrastructure implementations:
  - `GenericRepository<TEntity>`
  - `ApplicationDbContext` now also implements `IUnitOfWork`
- Added MediatR pipeline behaviors:
  - `LoggingBehavior`
  - `AuthorizationBehavior` with `IAuthorizableRequest`
  - Existing `ValidationBehavior`
- Updated dependency injection to register:
  - open generic repository
  - unit of work
  - full behavior chain

## Suggested next migration (non-breaking, incremental)

1. Split each feature in `HRM.Application` into `Commands`, `Queries`, `DTOs`.
2. Move EF model configurations to `Infrastructure/Persistence/Configurations`.
3. Add API filters/attributes for permission-based authorization.
4. Introduce `HRM.Tests` with unit/integration/functional test projects.
5. Move request-specific authorization from handlers into `IAuthorizableRequest` gradually.
