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

### Project Structure (45 files)

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

```bash
# Build
dotnet build src/Umbraco.Cms.Api.Common/Umbraco.Cms.Api.Common.csproj

# Pack for NuGet
dotnet pack src/Umbraco.Cms.Api.Common/Umbraco.Cms.Api.Common.csproj -c Release

# Run tests (integration tests in consuming APIs)
dotnet test tests/Umbraco.Tests.Integration/

# Check for outdated/vulnerable packages
dotnet list src/Umbraco.Cms.Api.Common/Umbraco.Cms.Api.Common.csproj package --outdated
dotnet list src/Umbraco.Cms.Api.Common/Umbraco.Cms.Api.Common.csproj package --vulnerable
```

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

### Schema ID Sanitization (OpenApi/SchemaIdHandler.cs:32)

```csharp
// Remove invalid characters to prevent OpenAPI generation errors
return Regex.Replace(name, @"[^\w]", string.Empty);

// Add "Model" suffix to avoid TypeScript name clashes (line 24)
if (name.EndsWith("Model") == false)
{
    name = $"{name}Model";
}
```

### Polymorphic Deserialization (Serialization/UmbracoJsonTypeInfoResolver.cs:31-34)

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

**Reference Tokens over JWT** (line 73-74):
```csharp
options
    .UseReferenceAccessTokens()
    .UseReferenceRefreshTokens();
```

**Why**: More secure (revocable), better for load balancing, uses ASP.NET Core Data Protection.

**Token Lifetime** (line 84-85):
```csharp
// Access token: 25% of refresh token lifetime
options.SetAccessTokenLifetime(new TimeSpan(timeOut.Ticks / 4));
options.SetRefreshTokenLifetime(timeOut);
```

**PKCE Required** (line 54-56):
```csharp
options
    .AllowAuthorizationCodeFlow()
    .RequireProofKeyForCodeExchange();
```

**Endpoints**:
- Backoffice: `/umbraco/management/api/v1/security/*`
- Member: `/umbraco/member/api/v1/security/*`

---

## 6. Common Issues & Edge Cases

### Polymorphic Deserialization Requires `$type`

**Issue**: Deserializing to an interface without `$type` discriminator fails.

**Handled in** (Json/NamedSystemTextJsonInputFormatter.cs:24-29):
```csharp
catch (NotSupportedException exception)
{
    // This happens when trying to deserialize to an interface,
    // without sending the $type as part of the request
    context.ModelState.TryAddModelException(string.Empty,
        new InputFormatterException(exception.Message, exception));
}
```

**Solution**: Clients must include `$type` property for interface types, or use concrete types.

### Schema ID Collisions with TypeScript

**Issue**: Type names like `Document` clash with TypeScript built-ins.

**Solution** (OpenApi/SchemaIdHandler.cs:24-29):
```csharp
if (name.EndsWith("Model") == false)
{
    // Add "Model" postfix to all models
    name = $"{name}Model";
}
```

### Generic Type Handling

**Issue**: `PagedViewModel<T>` needs flattened schema name.

**Solution** (OpenApi/SchemaIdHandler.cs:41-49):
```csharp
// Turns "PagedViewModel<RelationItemViewModel>" into "PagedRelationItemModel"
return $"{name}{string.Join(string.Empty, type.GenericTypeArguments.Select(SanitizedTypeName))}";
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

### Why Reference Tokens Instead of JWT?

**Decision**: Use `UseReferenceAccessTokens()` and ASP.NET Core Data Protection.

**Tradeoffs**:
- ✅ **Pros**: Revocable, simpler key management, better security
- ❌ **Cons**: Requires database lookup (slower than JWT), needs shared Data Protection key ring

**Load Balancing Requirement**: All servers must share the same Data Protection key ring and application name.

### Why Virtual Handlers?

**Decision**: Make `SchemaIdHandler`, `OperationIdHandler`, etc. virtual.

**Why**: Management API and Delivery API have different schema ID requirements. Virtual methods allow override without rewriting the entire handler.

**Example**: Management API might prefix all schemas with "Management", Delivery API with "Delivery".

### Performance: Subtype Caching

**Implementation** (Serialization/UmbracoJsonTypeInfoResolver.cs:14):
```csharp
private readonly ConcurrentDictionary<Type, ISet<Type>> _subTypesCache = new();
```

**Why**: Reflection is expensive. Cache discovered subtypes to avoid repeated `ITypeFinder.FindClassesOfType()` calls.

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

**HTTPS** (Configuration/ConfigureOpenIddict.cs:14):
```csharp
// Disable transport security requirement for local development
options.DisableTransportSecurityRequirement = _globalSettings.Value.UseHttps is false;
```

**⚠️ Warning**: Never disable HTTPS in production.

### Usage by Consuming APIs

**Registration Pattern**:
```csharp
// In Umbraco.Cms.Api.Management or Umbraco.Cms.Api.Delivery
builder
    .AddUmbracoApiOpenApiUI()    // Swagger + custom handlers
    .AddUmbracoOpenIddict();      // OAuth 2.0 authentication
```

---

## Quick Reference

### Essential Commands

```bash
# Build
dotnet build src/Umbraco.Cms.Api.Common/Umbraco.Cms.Api.Common.csproj

# Pack for NuGet
dotnet pack src/Umbraco.Cms.Api.Common/Umbraco.Cms.Api.Common.csproj -c Release

# Test via integration tests
dotnet test tests/Umbraco.Tests.Integration/
```

### Key Classes

| Class | Purpose | File |
|-------|---------|------|
| `ProblemDetailsBuilder` | Build RFC 7807 error responses | Builders/ProblemDetailsBuilder.cs |
| `SchemaIdHandler` | Generate OpenAPI schema IDs | OpenApi/SchemaIdHandler.cs |
| `UmbracoJsonTypeInfoResolver` | Polymorphic JSON serialization | Serialization/UmbracoJsonTypeInfoResolver.cs |
| `UmbracoBuilderAuthExtensions` | Configure OpenIddict | DependencyInjection/UmbracoBuilderAuthExtensions.cs |
| `PagedViewModel<T>` | Generic pagination model | ViewModels/Pagination/PagedViewModel.cs |

### Important Files

- `Umbraco.Cms.Api.Common.csproj` - Project dependencies
- `DependencyInjection/UmbracoBuilderApiExtensions.cs` - OpenAPI registration (line 12-30)
- `DependencyInjection/UmbracoBuilderAuthExtensions.cs` - OpenIddict setup (line 19-144)
- `Security/Paths.cs` - API endpoint path constants

### Getting Help

- **Root documentation**: `/CLAUDE.md` - Repository overview
- **Core patterns**: `/src/Umbraco.Core/CLAUDE.md` - Core contracts and patterns
- **Official docs**: https://docs.umbraco.com/
- **OpenIddict docs**: https://documentation.openiddict.com/

---

**This library is the foundation for all Umbraco CMS REST APIs. Focus on OpenAPI customization, authentication configuration, and polymorphic serialization when working here.**
