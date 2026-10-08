---
name: internal-class-needs-public-constructor-for-di
description: "An internal repository/service class registered with plain AddUnique<TInterface, TImpl>() needs a public constructor even though the class itself stays internal — Microsoft.Extensions.DependencyInjection's default container only reflects over public constructors"
metadata: 
  node_type: memory
  type: feedback
  originSessionId: 6b21014d-456d-4e72-9697-bec2de885da6
  modified: 2026-08-10T06:55:19.634Z
---

`AsyncDocumentRepository`/`AsyncDocumentBlueprintRepository` were registered in DI (`builder.Services.AddUnique<IAsyncDocumentRepository, AsyncDocumentRepository>()`), and the repository's own integration tests (which construct it manually via `new AsyncDocumentRepository(...)`) all passed — but the first real DI-constructed consumer (`DocumentUrlService`, once refactored to depend on `IAsyncDocumentRepository`) failed at runtime with `System.InvalidOperationException: A suitable constructor for type 'AsyncDocumentRepository' could not be located`.

**Why:** the constructor was declared `internal`. `Microsoft.Extensions.DependencyInjection`'s default `ServiceProvider` only discovers **public** constructors via reflection (`Type.GetConstructors()` with default flags), regardless of the containing class's own accessibility. An `internal` constructor on an `internal` class is invisible to it. This bug was invisible for a long time because nothing had ever actually asked the DI container to construct the type — every existing test used manual `new(...)` construction, which bypasses the container's constructor-discovery logic entirely.

Confirmed this is the established, working convention already used by every OTHER `internal` repository in this codebase registered the same way — e.g. `ContentTypeRepository` is `internal sealed class ContentTypeRepository` with a `public ContentTypeRepository(...)` constructor. The class being `internal` already prevents external assemblies from doing `new ContentTypeRepository(...)`; the constructor itself being `public` is what lets the DI container inside the same process construct it.

**How to apply:** When adding DI registration (`AddUnique<TInterface, TImpl>()`, `AddSingleton<TInterface, TImpl>()`, etc., without an explicit factory lambda) for any `internal` class in this codebase, its constructor must be `public`, not `internal`. If a repository/service class only has manual-construction test coverage (no DI-registered consumer yet), that test coverage does NOT prove the class is DI-constructible — write or find at least one test that resolves the type via the real DI container (`GetRequiredService<T>()` against a composed `UmbracoBuilder`) before trusting that registration works.
