# Umbraco.Cms.Api.Delivery

Headless content delivery REST API for Umbraco CMS. Enables frontend applications to fetch published content, media, and member-protected resources.

---

## 1. Architecture

**Type**: Class Library (NuGet Package)
**Target Framework**: .NET 10.0
**Purpose**: Content Delivery API for headless CMS scenarios

### Key Technologies

- **ASP.NET Core** - Web framework
- **OpenIddict** - Member authentication (OAuth 2.0)
- **Asp.Versioning** - API versioning (V1, V2)
- **Output Caching** - Configurable response caching
- **Examine/Lucene** - Content querying

### Dependencies

- `Umbraco.Cms.Api.Common` - Shared API infrastructure (OpenAPI, auth)
- `Umbraco.Web.Common` - Web functionality

### Project Structure (86 files)

```
Umbraco.Cms.Api.Delivery/
├── Controllers/
│   ├── Content/               # Content endpoints (by ID, route, query)
│   ├── Media/                 # Media endpoints (by ID, path, query)
│   └── Security/              # Member auth (authorize, token, signout)
├── Querying/
│   ├── Filters/               # ContentType, Name, CreateDate, UpdateDate
│   ├── Selectors/             # Ancestors, Children, Descendants
│   └── Sorts/                 # Name, CreateDate, UpdateDate, Level, SortOrder
├── Indexing/                  # Lucene index field handlers
├── Services/                  # Business logic and query building
├── Caching/                   # Output cache policies
├── Rendering/                 # Output expansion strategies
├── Configuration/             # OpenAPI configuration
└── Filters/                   # Action filters (access, validation)
```

### Design Patterns

1. **Strategy Pattern** - Query handlers (`ISelectorHandler`, `IFilterHandler`, `ISortHandler`)
2. **Factory Pattern** - `ApiContentQueryFactory` builds Examine queries
3. **Template Method** - `ContentApiControllerBase` for shared controller logic
4. **Options Pattern** - `DeliveryApiSettings` for all configuration

---

## 2. Commands

See "Quick Reference" section at bottom for common commands.

---

## 3. Key Patterns

### API Versioning (V1 vs V2)

**V1** (legacy) and **V2** (current) coexist. Key difference is output expansion:

```csharp
// DependencyInjection/UmbracoBuilderExtensions.cs:49-52
// V1 uses RequestContextOutputExpansionStrategy
// V2+ uses RequestContextOutputExpansionStrategyV2
return apiVersion.MajorVersion == 1
    ? provider.GetRequiredService<RequestContextOutputExpansionStrategy>()
    : provider.GetRequiredService<RequestContextOutputExpansionStrategyV2>();
```

**Why V2**: Improved `expand` and `fields` query parameter parsing (tree-based).

### Query System Architecture

Content querying flows through handlers registered in DI:

1. **Selectors** (`fetch` parameter): `ancestors:id`, `children:id`, `descendants:id`
2. **Filters** (`filter[]` parameter): `contentType:alias`, `name:value`, `createDate>2024-01-01`
3. **Sorts** (`sort[]` parameter): `name:asc`, `createDate:desc`, `level:asc`

```csharp
// Services/ApiContentQueryService.cs:91-96
ISelectorHandler? selectorHandler = _selectorHandlers.FirstOrDefault(h => h.CanHandle(fetch));
return selectorHandler?.BuildSelectorOption(fetch);
```

### Path Decoding Workaround

ASP.NET Core doesn't decode forward slashes in route parameters:

```csharp
// Controllers/DeliveryApiControllerBase.cs:21-31
// OpenAPI clients URL-encode paths, but ASP.NET Core doesn't decode "/"
// See https://github.com/dotnet/aspnetcore/issues/11544
if (path.Contains("%2F", StringComparison.OrdinalIgnoreCase))
{
    path = WebUtility.UrlDecode(path);
}
```

---

## 4. Testing

**Location**: No direct tests - tested via integration tests in test projects

```bash
dotnet test tests/Umbraco.Tests.Integration/ --filter "FullyQualifiedName~Delivery"
```

**Internals exposed to** (csproj lines 28-36):
- `Umbraco.Tests.UnitTests`
- `Umbraco.Tests.Integration`
- `DynamicProxyGenAssembly2` (for mocking)

**Focus areas**:
- Query parsing (selectors, filters, sorts)
- Member authentication flows
- Output caching behavior
- Protected content access

---

## 5. Security & Access Control

### Three Access Modes

```csharp
// Services/ApiAccessService.cs:21-27
public bool HasPublicAccess() => _deliveryApiSettings.PublicAccess || HasValidApiKey();
public bool HasPreviewAccess() => HasValidApiKey();
public bool HasMediaAccess() => _deliveryApiSettings is { PublicAccess: true, Media.PublicAccess: true } || HasValidApiKey();
```

**Access levels**:
1. **Public** - No authentication required (if enabled)
2. **API Key** - Via `Api-Key` header
3. **Preview** - Always requires API key

### Member Authentication

OpenIddict-based OAuth 2.0 for member-protected content:

**Flows supported** (Controllers/Security/MemberController.cs):
- Authorization Code + PKCE (line 53)
- Client Credentials (line 112)
- Refresh Token (line 98)

**Endpoints**:
- `GET /umbraco/delivery/api/v1/security/member/authorize`
- `POST /umbraco/delivery/api/v1/security/member/token`
- `GET /umbraco/delivery/api/v1/security/member/signout`

**Scopes**: Only `openid` and `offline_access` allowed for members (line 220-222)

### Protected Content

Member access checked via `ProtectedAccess` model:

```csharp
// Controllers/Content/QueryContentApiController.cs:56-57
ProtectedAccess protectedAccess = await _requestMemberAccessService.MemberAccessAsync();
Attempt<PagedModel<Guid>, ApiContentQueryOperationStatus> queryAttempt =
    _apiContentQueryService.ExecuteQuery(fetch, filter, sort, protectedAccess, skip, take);
```

---

## 6. Output Caching

### Cache Policy Configuration

```csharp
// DependencyInjection/UmbracoBuilderExtensions.cs:120-136
// Content and Media have separate cache durations
options.AddPolicy(
    Constants.DeliveryApi.OutputCache.ContentCachePolicy,
    new DeliveryApiOutputCachePolicy(
        outputCacheSettings.ContentDuration,
        new StringValues([AcceptLanguage, AcceptSegment, StartItem])));
```

**Cache invalidation conditions** (Caching/DeliveryApiOutputCachePolicy.cs:31):
```csharp
// Never cache preview or non-public access
context.EnableOutputCaching = requestPreviewService.IsPreview() is false
    && apiAccessService.HasPublicAccess();
```

**Vary by headers**: `Accept-Language`, `Accept-Segment`, `Start-Item`

---

## 7. Edge Cases & Known Issues

### Technical Debt (TODOs in codebase)

1. **V1 Removal Pending** (2 locations):
   - `DependencyInjection/UmbracoBuilderExtensions.cs:98` - FIXME: remove matcher policy
   - `Routing/DeliveryApiItemsEndpointsMatcherPolicy.cs:11` - FIXME: remove class

2. **Obsolete Reference Warnings** (csproj:9-13):
   - `ASP0019` - IHeaderDictionary.Append usage
   - `CS0618/CS0612` - Obsolete member references

### Empty Query Results

Query service returns empty results (not errors) for invalid options:

```csharp
// Services/ApiContentQueryService.cs:54-78
// Invalid selector/filter/sort returns fail status with empty result
return Attempt.FailWithStatus(ApiContentQueryOperationStatus.SelectorOptionNotFound, emptyResult);
```

### Start Item Fallback

When no `fetch` parameter provided, uses start item or all content:

```csharp
// Services/ApiContentQueryService.cs:99-112
if (_requestStartItemProviderAccessor.TryGetValue(out IRequestStartItemProvider? requestStartItemProvider))
{
    IPublishedContent? startItem = requestStartItemProvider.GetStartItem();
    // Use descendants of start item
}
return _apiContentQueryProvider.AllContentSelectorOption(); // Fallback to all
```

---

## 8. Project-Specific Notes

### V1 vs V2 Differences

| Feature | V1 | V2 |
|---------|----|----|
| Output expansion | Basic | Tree-based parsing |
| `expand` parameter | Flat list | Nested syntax |
| `fields` parameter | Limited | Full property selection |
| Default expansion strategy | `RequestContextOutputExpansionStrategy` | `RequestContextOutputExpansionStrategyV2` |

**Migration note**: V1 is deprecated; plan removal when V17+ drops V1 support.

### JSON Configuration

Delivery API has its own JSON options (distinct from Management API):

```csharp
// DependencyInjection/UmbracoBuilderExtensions.cs:82-88
.AddJsonOptions(Constants.JsonOptionsNames.DeliveryApi, options =>
{
    options.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
    options.JsonSerializerOptions.TypeInfoResolver = new DeliveryApiJsonTypeResolver();
    options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
});
```

### Member Token Revocation

Tokens automatically revoked on member changes:

```csharp
// DependencyInjection/UmbracoBuilderExtensions.cs:93-96
builder.AddNotificationAsyncHandler<MemberSavedNotification, RevokeMemberAuthenticationTokensNotificationHandler>();
builder.AddNotificationAsyncHandler<MemberDeletedNotification, RevokeMemberAuthenticationTokensNotificationHandler>();
builder.AddNotificationAsyncHandler<AssignedMemberRolesNotification, RevokeMemberAuthenticationTokensNotificationHandler>();
builder.AddNotificationAsyncHandler<RemovedMemberRolesNotification, RevokeMemberAuthenticationTokensNotificationHandler>();
```

### External Dependencies

**Examine/Lucene** (via Core):
- Powers content querying
- Selector/Filter/Sort handlers build Lucene queries

**OpenIddict** (via Api.Common):
- Member OAuth 2.0 authentication
- Reference tokens (not JWT)

### Configuration (appsettings.json)

```json
{
  "Umbraco": {
    "CMS": {
      "DeliveryApi": {
        "Enabled": true,
        "PublicAccess": true,
        "ApiKey": "your-api-key",
        "Media": {
          "Enabled": true,
          "PublicAccess": true
        },
        "MemberAuthorization": {
          "AuthorizationCodeFlow": { "Enabled": true },
          "ClientCredentialsFlow": { "Enabled": false }
        },
        "OutputCache": {
          "Enabled": true,
          "ContentDuration": "00:01:00",
          "MediaDuration": "00:01:00"
        }
      }
    }
  }
}
```

### API Endpoints Summary

**Content** (`/umbraco/delivery/api/v2/content`):
- `GET /item/{id}` - Single content by GUID
- `GET /item/{path}` - Single content by route
- `GET /items` - Multiple by IDs
- `GET /` - Query with fetch/filter/sort

**Media** (`/umbraco/delivery/api/v2/media`):
- `GET /item/{id}` - Single media by GUID
- `GET /item/{path}` - Single media by path
- `GET /items` - Multiple by IDs
- `GET /` - Query media

**Security** (`/umbraco/delivery/api/v1/security/member`):
- `GET /authorize` - Start OAuth flow
- `POST /token` - Exchange code for token
- `GET /signout` - Revoke session

---

## Quick Reference

### Essential Commands

```bash
# Build project
dotnet build src/Umbraco.Cms.Api.Delivery/Umbraco.Cms.Api.Delivery.csproj

# Pack for NuGet
dotnet pack src/Umbraco.Cms.Api.Delivery/Umbraco.Cms.Api.Delivery.csproj -c Release

# Run integration tests
dotnet test tests/Umbraco.Tests.Integration/ --filter "FullyQualifiedName~Delivery"

# Check packages
dotnet list src/Umbraco.Cms.Api.Delivery/Umbraco.Cms.Api.Delivery.csproj package --outdated
```

### Key Classes

| Class | Purpose | File |
|-------|---------|------|
| `DeliveryApiControllerBase` | Base controller with path decoding | Controllers/DeliveryApiControllerBase.cs |
| `ApiContentQueryService` | Query orchestration | Services/ApiContentQueryService.cs |
| `ApiAccessService` | Access control logic | Services/ApiAccessService.cs |
| `DeliveryApiOutputCachePolicy` | Cache policy implementation | Caching/DeliveryApiOutputCachePolicy.cs |
| `MemberController` | OAuth endpoints | Controllers/Security/MemberController.cs |
| `RequestContextOutputExpansionStrategyV2` | V2 output expansion | Rendering/RequestContextOutputExpansionStrategyV2.cs |

### Important Files

- `Umbraco.Cms.Api.Delivery.csproj` - Project dependencies
- `DependencyInjection/UmbracoBuilderExtensions.cs` - DI registration (lines 33-141)
- `Configuration/DeliveryApiConfiguration.cs` - API constants
- `Services/ApiContentQueryService.cs` - Query execution

### Getting Help

- **Root documentation**: `/CLAUDE.md` - Repository overview
- **API Common patterns**: `/src/Umbraco.Cms.Api.Common/CLAUDE.md`
- **Official docs**: https://docs.umbraco.com/umbraco-cms/reference/content-delivery-api
- **Media docs**: https://docs.umbraco.com/umbraco-cms/reference/content-delivery-api/media-delivery-api

---

**This library exposes Umbraco content and media via REST for headless scenarios. Focus on query handlers, access control, and member authentication when working here.**
