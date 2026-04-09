# Umbraco.Cms.Api.Common

Shared infrastructure for Umbraco CMS REST APIs (Management and Delivery).

---

## 1. Architecture

**Type**: Class Library (NuGet Package)
**Target Framework**: .NET 10.0
**Purpose**: Common API infrastructure - OpenAPI/Swagger, JSON serialization, OpenIddict authentication, problem details

### Key Technologies

- **ASP.NET Core** - Web framework
- **Swashbuckle** - OpenAPI/Swagger documentation generation
- **OpenIddict** - OAuth 2.0/OpenID Connect authentication
- **Asp.Versioning** - API versioning
- **System.Text.Json** - Polymorphic JSON serialization

### Dependencies

- `Umbraco.Core` - Domain models and service contracts
- `Umbraco.Web.Common` - Web functionality

### Project Structure (46 files)

```
Umbraco.Cms.Api.Common/
├── OpenApi/                    # Schema/Operation ID handlers for Swagger
│   ├── SchemaIdHandler.cs     # Generates schema IDs (e.g., "PagedUserModel")
│   ├── OperationIdHandler.cs  # Generates operation IDs
│   └── SubTypesHandler.cs     # Polymorphism support
├── Serialization/              # JSON type resolution
│   └── UmbracoJsonTypeInfoResolver.cs
├── Configuration/              # Options configuration
│   ├── ConfigureUmbracoSwaggerGenOptions.cs
│   └── ConfigureOpenIddict.cs
├── DependencyInjection/        # Service registration
│   ├── UmbracoBuilderApiExtensions.cs
│   └── UmbracoBuilderAuthExtensions.cs
├── Builders/                   # RFC 7807 problem details
│   └── ProblemDetailsBuilder.cs
├── ViewModels/Pagination/      # Common DTOs
└── Security/                   # Auth paths and handlers
```

### Design Patterns

1. **Strategy Pattern** - `ISchemaIdHandler`, `IOperationIdHandler` (extensible via inheritance)
2. **Builder Pattern** - `ProblemDetailsBuilder` for fluent error responses
3. **Options Pattern** - All configuration via `IConfigureOptions<T>`

---

## 2. Commands

See "Quick Reference" section at bottom for common commands.

---

## 3. Key Patterns

### Virtual Handlers for Extensibility

Handlers are intentionally virtual to allow consuming APIs to override:

```csharp
// NOTE: Left unsealed on purpose, so it is extendable.
public class SchemaIdHandler : ISchemaIdHandler
{
    public virtual bool CanHandle(Type type) { }
    public virtual string Handle(Type type) { }
}
```

**Why**: Management and Delivery APIs can customize schema/operation ID generation.

### Schema ID Sanitization (OpenApi/SchemaIdHandler.cs:24-29, 32)

```csharp
// Add "Model" suffix to avoid TypeScript name clashes (lines 24-29)
if (name.EndsWith("Model") == false)
{
    // because some models names clash with common classes in TypeScript (i.e. Document),
    // we need to add a "Model" postfix to all models
    name = $"{name}Model";
}

// Remove invalid characters to prevent OpenAPI generation errors (line 32)
return Regex.Replace(name, @"[^\w]", string.Empty);
```

### Polymorphic Deserialization (Serialization/UmbracoJsonTypeInfoResolver.cs:29-35)

```csharp
// IMPORTANT: do NOT return an empty enumerable here. it will cause nullability to fail on reference
// properties, because "$ref" does not mix and match well with "nullable" in OpenAPI.
if (type.IsInterface is false)
{
    return new[] { type };
}
```

**Why**: Interfaces must return concrete types to avoid OpenAPI schema conflicts.

---

## 4. Testing

**Location**: No direct tests - tested via integration tests in consuming APIs

**How to test changes**:
```bash
# Run integration tests that exercise this library
dotnet test tests/Umbraco.Tests.Integration/

# Verify OpenAPI generation
# 1. Run Management API
# 2. Navigate to /umbraco/swagger/
# 3. Check schema IDs and operation IDs
```

**Focus areas when testing**:
- OpenAPI document generation (schema IDs, operation IDs)
- Polymorphic JSON serialization/deserialization
- OpenIddict authentication flow
- Problem details formatting

---

## 5. OpenIddict Authentication

### Key Configuration (DependencyInjection/UmbracoBuilderAuthExtensions.cs)

**Reference Tokens over JWT** (line 76-80):
```csharp
// Enable reference tokens
// - see https://documentation.openiddict.com/configuration/token-storage.html
options
    .UseReferenceAccessTokens()
    .UseReferenceRefreshTokens();
```

**Why**: More secure (revocable), better for load balancing, uses ASP.NET Core Data Protection.

**Token Lifetime** (line 88-91):
```csharp
// Make the access token lifetime 25% of the refresh token lifetime
options.SetAccessTokenLifetime(new TimeSpan(timeOut.Ticks / 4));
options.SetRefreshTokenLifetime(timeOut);
```

**PKCE Required** (line 59-63):
```csharp
// Enable authorization code flow with PKCE
options
    .AllowAuthorizationCodeFlow()
    .RequireProofKeyForCodeExchange()
    .AllowRefreshTokenFlow();
```

**Endpoints**: Backoffice `/umbraco/management/api/v1/security/*`, Member `/umbraco/member/api/v1/security/*`

### Secure Cookie-Based Token Storage (v17+)

**Implementation** (DependencyInjection/HideBackOfficeTokensHandler.cs):

Back-office tokens are hidden from client-side JavaScript via HTTP-only cookies:

```csharp
private const string AccessTokenCookieKey = "__Host-umbAccessToken";
private const string RefreshTokenCookieKey = "__Host-umbRefreshToken";

// Tokens are encrypted via Data Protection and stored in cookies
SetCookie(httpContext, AccessTokenCookieKey, context.Response.AccessToken);
context.Response.AccessToken = "[redacted]";  // Client sees redacted value
```

**Key Security Features** (lines 143-165): `HttpOnly`, `IsEssential`, `Path="/"`, `Secure` (HTTPS), `__Host-` prefix

**Configuration**: `BackOfficeTokenCookieSettings.Enabled` (default: true in v17+)

**Implications**: Client-side cannot access tokens; encrypted with Data Protection; load balancing needs shared key ring; API requests need `credentials: include`

---

## 6. Common Issues & Edge Cases

### Polymorphic Deserialization Requires `$type`

**Issue**: Deserializing to an interface without `$type` discriminator fails.

**Handled in** (Json/NamedSystemTextJsonInputFormatter.cs:24-29):
```csharp
catch (NotSupportedException exception)
{
    // This happens when trying to deserialize to an interface, without sending the $type as part of the request
    context.ModelState.TryAddModelException(string.Empty, new InputFormatterException(exception.Message, exception));
    return await InputFormatterResult.FailureAsync();
}
```

**Solution**: Clients must include `$type` property for interface types, or use concrete types.

### Schema ID Collisions with TypeScript

**Issue**: Type names like `Document` clash with TypeScript built-ins.

**Solution**: Add "Model" suffix (OpenApi/SchemaIdHandler.cs:24-29)

### Generic Type Handling

**Issue**: `PagedViewModel<T>` needs flattened schema name.

**Solution** (OpenApi/SchemaIdHandler.cs:41-50):
```csharp
private string HandleGenerics(string name, Type type)
{
    if (!type.IsGenericType)
        return name;

    // use attribute custom name or append the generic type names
    // turns "PagedViewModel<RelationItemViewModel>" into "PagedRelationItem"
    return $"{name}{string.Join(string.Empty, type.GenericTypeArguments.Select(SanitizedTypeName))}";
}
```

---

## 7. Extending This Library

### Adding a Custom OpenAPI Handler

1. **Implement interface**:
   ```csharp
   public class MySchemaIdHandler : SchemaIdHandler
   {
       public override bool CanHandle(Type type)
           => type.Namespace?.StartsWith("MyProject") is true;

       public override string Handle(Type type)
           => $"My{base.Handle(type)}";
   }
   ```

2. **Register in consuming API**:
   ```csharp
   builder.Services.AddSingleton<ISchemaIdHandler, MySchemaIdHandler>();
   ```

**Note**: Handlers registered later take precedence in the selector.

### Customizing Problem Details

```csharp
var problemDetails = new ProblemDetailsBuilder()
    .WithTitle("Validation Failed")
    .WithDetail("The request contains errors")
    .WithType("ValidationError")
    .WithOperationStatus(MyOperationStatus.ValidationFailed)
    .WithRequestModelErrors(errors)
    .Build();

return BadRequest(problemDetails);
```

---

## 8. Project-Specific Notes

### Why Virtual Handlers?

**Decision**: Make `SchemaIdHandler`, `OperationIdHandler`, etc. virtual.

**Why**: Management API and Delivery API have different schema ID requirements. Virtual methods allow override without rewriting the entire handler.

**Example**: Management API might prefix all schemas with "Management", Delivery API with "Delivery".

### Performance: Subtype Caching

**Why**: Cache discovered subtypes (UmbracoJsonTypeInfoResolver.cs:14) to avoid expensive reflection calls

### Known Limitations

1. **Polymorphic Deserialization**:
   - Requires `$type` discriminator in JSON for interfaces
   - Only discovers types in Umbraco namespaces
   - Not all .NET types are discoverable

2. **OpenAPI Schema Generation**:
   - Generic types are flattened (e.g., `PagedViewModel<T>` → `PagedTModel`)
   - Type names may need "Model" suffix to avoid clashes

3. **OpenIddict Multi-Server**:
   - Requires shared Data Protection key ring
   - All servers must have synchronized clocks (NTP)
   - Reference tokens require database storage

### External Dependencies

**OpenIddict**:
- OAuth 2.0 / OpenID Connect provider
- Version: See `Directory.Packages.props`
- Uses ASP.NET Core Data Protection for token encryption

**Swashbuckle**:
- OpenAPI 3.0 document generation
- Custom filters: `EnumSchemaFilter`, `MimeTypeDocumentFilter`, `RemoveSecuritySchemesDocumentFilter`

**Asp.Versioning**:
- API versioning via `ApiVersion` attribute
- API explorer integration for multi-version Swagger docs

### Configuration

**HTTPS**: `DisableTransportSecurityRequirement` for local dev only (ConfigureOpenIddict.cs:14). **Warning**: Never disable in production.

### Usage Pattern

Consuming APIs call `builder.AddUmbracoApiOpenApiUI().AddUmbracoOpenIddict()`

---

## Quick Reference

### Essential Commands

```bash
# Build project
dotnet build src/Umbraco.Cms.Api.Common/Umbraco.Cms.Api.Common.csproj

# Pack for NuGet
dotnet pack src/Umbraco.Cms.Api.Common/Umbraco.Cms.Api.Common.csproj -c Release

# Test via integration tests
dotnet test tests/Umbraco.Tests.Integration/

# Check packages
dotnet list src/Umbraco.Cms.Api.Common/Umbraco.Cms.Api.Common.csproj package --outdated
dotnet list src/Umbraco.Cms.Api.Common/Umbraco.Cms.Api.Common.csproj package --vulnerable
```

### Key Classes

| Class | Purpose | File |
|-------|---------|------|
| `ProblemDetailsBuilder` | Build RFC 7807 error responses | Builders/ProblemDetailsBuilder.cs |
| `SchemaIdHandler` | Generate OpenAPI schema IDs | OpenApi/SchemaIdHandler.cs |
| `UmbracoJsonTypeInfoResolver` | Polymorphic JSON serialization | Serialization/UmbracoJsonTypeInfoResolver.cs |
| `UmbracoBuilderAuthExtensions` | Configure OpenIddict | DependencyInjection/UmbracoBuilderAuthExtensions.cs |
| `HideBackOfficeTokensHandler` | Secure cookie-based token storage | DependencyInjection/HideBackOfficeTokensHandler.cs |
| `PagedViewModel<T>` | Generic pagination model | ViewModels/Pagination/PagedViewModel.cs |

### Important Files

- `Umbraco.Cms.Api.Common.csproj` - Project dependencies
- `DependencyInjection/UmbracoBuilderApiExtensions.cs` - OpenAPI registration (line 12-31)
- `DependencyInjection/UmbracoBuilderAuthExtensions.cs` - OpenIddict setup (line 20-183)
- `Security/Paths.cs` - API endpoint path constants

### Getting Help

- **Root documentation**: `/CLAUDE.md` - Repository overview
- **Core patterns**: `/src/Umbraco.Core/CLAUDE.md` - Core contracts and patterns
- **Official docs**: https://docs.umbraco.com/
- **OpenIddict docs**: https://documentation.openiddict.com/

---

**This library is the foundation for all Umbraco CMS REST APIs. Focus on OpenAPI customization, authentication configuration, and polymorphic serialization when working here.**
