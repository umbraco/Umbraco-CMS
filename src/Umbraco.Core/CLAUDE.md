# Umbraco.Core Project Guide

## Project Overview

**Umbraco.Core** is the foundational layer of Umbraco CMS, containing the core domain models, services interfaces, events/notifications system, and essential abstractions. This project has NO database implementation and minimal external dependencies - it focuses on defining contracts and business logic.

**Package ID**: `Umbraco.Cms.Core`  
**Namespace**: `Umbraco.Cms.Core`

### What Lives Here vs. Other Projects

- **Umbraco.Core**: Interfaces, models, notifications, service contracts, core business logic, configuration models
- **Umbraco.Infrastructure**: Concrete implementations (repositories, database access, file systems, concrete services)
- **Umbraco.Web.Common**: Web-specific functionality (controllers, middleware, routing)
- **Umbraco.Cms.Api.Common**: API endpoints and DTOs

## Key Directory Structure

### Core Domain Areas

```
/Models                     - Domain models and DTOs
├── /Entities              - Base entity interfaces (IEntity, IUmbracoEntity, ITreeEntity)
├── /ContentEditing        - Content editing models and DTOs
├── /ContentPublishing     - Publishing-related models
├── /Membership            - User, member, and group models
├── /PublishedContent      - Published content abstractions
├── /DeliveryApi          - Delivery API models
└── /Blocks               - Block List/Grid models

/Services                   - Service interfaces (~237 files!)
├── /OperationStatus       - Enums for service operation results (48 status types)
├── /ContentTypeEditing    - Content type editing services
├── /Navigation            - Navigation services
└── /ImportExport          - Import/export services

/Notifications              - Event notification system (100+ notification types)
/Events                     - Event infrastructure and handlers
```

### Infrastructure & Patterns

```
/Composing                  - Composer pattern for DI registration
/DependencyInjection        - IUmbracoBuilder and DI extensions
/Scoping                    - Unit of work pattern (ICoreScopeProvider)
/Cache                      - Caching abstractions and refreshers
├── /Refreshers            - Cache invalidation for distributed systems
└── /NotificationHandlers  - Cache invalidation via notifications

/PropertyEditors            - Property editor abstractions
├── /ValueConverters       - Convert stored values to typed values
├── /Validation            - Property validation infrastructure
└── /DeliveryApi          - Delivery API property converters

/Persistence                - Repository interfaces (no implementations!)
├── /Repositories          - Repository contracts
└── /Querying              - Query abstractions
```

### Supporting Functionality

```
/Configuration              - Configuration models and settings
├── /Models                - Strongly-typed settings (50+ configuration classes)
└── /UmbracoSettings       - Legacy Umbraco settings

/Extensions                 - Extension methods (100+ extension files)
/Mapping                    - Object mapping abstractions (IUmbracoMapper)
/Serialization              - JSON serialization configuration
/Security                   - Security abstractions and authorization
/Routing                    - URL routing abstractions
/Templates                  - Template/view abstractions
/Webhooks                   - Webhook event system
/HealthChecks               - Health check abstractions
/Telemetry                  - Telemetry/analytics
/Install & /Installer       - Installation infrastructure
```

## Core Patterns and Conventions

### 1. Service Layer Pattern

Services follow a consistent structure:

```csharp
// Interface defines contract (in Umbraco.Core)
public interface IContentService
{
    IContent? GetById(Guid key);
    Task<Attempt<IContent?, ContentEditingOperationStatus>> CreateAsync(...);
}

// Implementation lives in Umbraco.Infrastructure
// Service operations return Attempt<T, TStatus> for typed results
```

**Key conventions**:
- Interfaces in Umbraco.Core, implementations in Umbraco.Infrastructure
- Use `Attempt<TResult, TStatus>` for operations that can fail with specific reasons
- OperationStatus enums provide detailed failure reasons
- Services are registered via DI in builder extensions

### 2. Notification System (Event Handling)

Umbraco uses a notification pattern instead of traditional events:

```csharp
// 1. Define notification (implements INotification)
public class ContentSavedNotification : INotification
{
    public IEnumerable<IContent> SavedEntities { get; }
}

// 2. Create handler
public class MyNotificationHandler : INotificationHandler<ContentSavedNotification>
{
    public void Handle(ContentSavedNotification notification)
    {
        // React to content being saved
    }
}

// 3. Add to appropriate builder extensions
builder.AddNotificationHandler<ContentSavedNotification, MyNotificationHandler>();
```

**Notification types**:
- `*SavingNotification` - Before save (cancellable via `ICancelableNotification`)
- `*SavedNotification` - After save
- `*DeletingNotification` / `*DeletedNotification`
- `*MovingNotification` / `*MovedNotification`

**Key interface**: `IEventAggregator` - publishes notifications to handlers

### 3. Composer Pattern (DI Registration)

Composers register services and configure the application, this is to make the system easily extendible by package developers and implementors.
Internally we only use this for temporary service registration for use in short lived code, for example migrations:

```csharp
public class MyComposer : IComposer
{
    public void Compose(IUmbracoBuilder builder)
    {
        // Register services
        builder.Services.AddSingleton<IMyService, MyService>();
        
        // Add to collections
        builder.PropertyEditors().Add<MyPropertyEditor>();
        
        // Register notification handlers
        builder.AddNotificationHandler<ContentSavedNotification, MyHandler>();
    }
}
```

**Composer ordering**:
- `[ComposeBefore(typeof(OtherComposer))]`
- `[ComposeAfter(typeof(OtherComposer))]`
- `[Weight(100)]` - lower runs first

### 4. Entity and Content Model Hierarchy

```
IEntity                     - Base: Id, Key, CreateDate, UpdateDate
  └─ IUmbracoEntity        - Adds: Name, CreatorId, ParentId, Path, Level, SortOrder
      └─ ITreeEntity       - Tree structure support
          └─ IContentBase  - Adds: Properties, ContentType, CultureInfos
              ├─ IContent  - Documents (publishable)
              ├─ IMedia    - Media items
              └─ IMember   - Members
```

**Key interfaces**:
- `IContentBase` - Common base for all content items (properties, cultures)
- `IContent` - Documents with publishing workflow
- `IContentType`, `IMediaType`, `IMemberType` - Define structure
- `IProperty` - Individual property on content
- `IPropertyType` - Property definition on content type

### 5. Scoping Pattern (Unit of Work)

```csharp
public class MyService
{
    private readonly ICoreScopeProvider _scopeProvider;
    
    public void DoWork()
    {
        using ICoreScope scope = _scopeProvider.CreateCoreScope();
        
        // Do database work
        // Access repositories
        
        scope.Complete(); // Commit transaction
    }
}
```

**Key points**:
- Scopes manage transactions and cache lifetime
- Must call `scope.Complete()` to commit
- Scopes can be nested (innermost controls transaction)
- `RepositoryCacheMode` controls caching behavior

### 6. Property Editors

Property editors define how data is edited and stored:

```csharp
public class MyPropertyEditor : IDataEditor
{
    public string Alias => "My.PropertyEditor";
    
    public IDataValueEditor GetValueEditor()
    {
        return new MyDataValueEditor();
    }
    
    public IConfigurationEditor GetConfigurationEditor()
    {
        return new MyConfigurationEditor();
    }
}
```

**Key interfaces**:
- `IDataEditor` - Property editor registration
- `IDataValueEditor` - Value editing and conversion
- `IPropertyIndexValueFactory` - Search indexing
- `IPropertyValueConverter` - Convert stored values to typed values

### 7. Cache Refreshers (Distributed Cache)

For multi-server deployments, cache refreshers synchronize cache:

```csharp
public class MyEntityCacheRefresher : CacheRefresherBase<MyEntityCacheRefresher>
{
    public override Guid RefresherUniqueId => new Guid("...");
    public override string Name => "My Entity Cache Refresher";
    
    public override void RefreshAll()
    {
        // Clear all cache
    }
    
    public override void Refresh(int id)
    {
        // Clear cache for specific entity
    }
}
```

## Important Base Classes and Interfaces

### Must-Know Abstractions

#### Service Layer
- `IContentService` - Content CRUD operations
- `IContentTypeService` - Content type management
- `IMediaService` - Media operations
- `IDataTypeService` - Data type configuration
- `IUserService` - User management
- `ILocalizationService` - Languages and dictionary
- `IRelationService` - Entity relationships

#### Content Models
- `IContent` / `IContentBase` - Document entities
- `IContentType` - Document type definition
- `IProperty` / `IPropertyType` - Property definitions
- `IPublishedContent` - Read-only published content

#### Infrastructure
- `ICoreScopeProvider` - Unit of work / transactions
- `IEventAggregator` - Notification publishing
- `IUmbracoMapper` - Object mapping
- `IComposer` / `IUmbracoBuilder` - DI registration

#### Entity Base Classes
- `EntityBase` - Basic entity with Id, Key, dates
- `TreeEntityBase` - Adds Name, hierarchy
- `BeingDirty` / `ICanBeDirty` - Change tracking

## Key Files and Constants

### Essential Files

1. **Constants.cs** (and 40 Constants-*.cs files)
   - `Constants.System.*` - System-level constants
   - `Constants.Security.*` - Security and authorization
   - `Constants.Conventions.*` - Naming conventions
   - `Constants.PropertyEditors.*` - Built-in property editor aliases
   - `Constants.ObjectTypes.*` - Entity type GUIDs
   - Use these instead of magic strings!

2. **Udi.cs** / **GuidUdi.cs** / **StringUdi.cs**
   - Umbraco Identifiers (like URIs): `umb://document/{guid}`
   - Used throughout for entity references
   - `UdiParser.Parse("umb://document/...")` to parse

3. **Attempt.cs** and **Attempt<TResult, TStatus>.cs**
   - Result pattern for operations that can fail
   - `Attempt.Succeed(value)` / `Attempt.Fail<T>()`
   - `Attempt<Content, ContentEditingOperationStatus>` - typed result with status

### Configuration

Configuration models in `/Configuration/Models`:
- `ContentSettings` - Content-related settings
- `GlobalSettings` - Global Umbraco settings
- `SecuritySettings` - Security configuration
- `DeliveryApiSettings` - Delivery API configuration
- Access via `IOptionsMonitor<TSettings>`

## Dependencies

### What Umbraco.Core Depends On
- Microsoft.Extensions.* - DI, Configuration, Logging, Caching, Options
- Microsoft.Extensions.Identity.Core - Identity infrastructure
- NO database dependencies
- NO web dependencies

### What Depends on Umbraco.Core
- **Umbraco.Infrastructure** - Implements all Core interfaces
- **Umbraco.PublishedCache.HybridCache** - Published content caching
- **Umbraco.Cms.Persistence.EFCore** - EF Core persistence
- **Umbraco.Cms.Api.Common** - API infrastructure
- All higher-level Umbraco projects

## Common Development Tasks

### 1. Creating a New Service

```csharp
// 1. Define interface in Umbraco.Core/Services/IMyService.cs
public interface IMyService
{
    Task<Attempt<MyResult, MyOperationStatus>> CreateAsync(...);
}

// 2. Define operation status in Services/OperationStatus/
public enum MyOperationStatus
{
    Success,
    NotFound,
    ValidationFailed
}

// 3. Implement in Umbraco.Infrastructure
// 4. Register in a Composer
builder.Services.AddScoped<IMyService, MyService>();
```

### 2. Adding a Notification Handler

```csharp
// 1. Identify notification (e.g., ContentSavingNotification)
// 2. Create handler
public class MyContentHandler : INotificationHandler<ContentSavingNotification>
{
    public void Handle(ContentSavingNotification notification)
    {
        foreach (var content in notification.SavedEntities)
        {
            // Validate, modify, or react
        }
        
        // Cancel if needed (for cancellable notifications)
        // notification.Cancel = true;
    }
}

// 3. Register in Composer
builder.AddNotificationHandler<ContentSavingNotification, MyContentHandler>();
```

### 3. Creating a Property Editor

```csharp
// 1. Define in Umbraco.Core/PropertyEditors/
[DataEditor("My.Alias", "My Editor", "view")]
public class MyPropertyEditor : DataEditor
{
    public MyPropertyEditor(IDataValueEditorFactory dataValueEditorFactory)
        : base(dataValueEditorFactory)
    { }
    
    protected override IDataValueEditor CreateValueEditor()
    {
        return DataValueEditorFactory.Create<MyValueEditor>(Attribute!);
    }
}

// 2. Register in Composer
builder.PropertyEditors().Add<MyPropertyEditor>();
```

### 4. Working with Content

```csharp
public class MyService
{
    private readonly IContentService _contentService;
    private readonly ICoreScopeProvider _scopeProvider;
    
    public async Task UpdateContentAsync(Guid key)
    {
        using var scope = _scopeProvider.CreateCoreScope();
        
        IContent? content = _contentService.GetById(key);
        if (content == null)
            return;
        
        // Modify content
        content.SetValue("propertyAlias", "new value");
        
        // Save (triggers notifications)
        var result = _contentService.Save(content);
        
        scope.Complete();
    }
}
```

### 5. Extending Content Types

```csharp
public class ContentTypeCustomizationComposer : IComposer
{
    public void Compose(IUmbracoBuilder builder)
    {
        builder.AddNotificationHandler<ContentTypeSavedNotification, MyHandler>();
    }
}

public class MyHandler : INotificationHandler<ContentTypeSavedNotification>
{
    public void Handle(ContentTypeSavedNotification notification)
    {
        foreach (var contentType in notification.SavedEntities)
        {
            // React to content type changes
        }
    }
}
```

### 6. Defining Configuration

```csharp
// 1. Create settings class in Configuration/Models/
public class MySettings
{
    public string ApiKey { get; set; } = string.Empty;
    public int MaxItems { get; set; } = 10;
}

// 2. Bind in Composer
builder.Services.Configure<MySettings>(
    builder.Config.GetSection("Umbraco:MySettings"));

// 3. Inject via IOptionsMonitor<MySettings>
```

## Testing Considerations

The project has `InternalsVisibleTo` attributes for:
- `Umbraco.Tests`
- `Umbraco.Tests.Common`
- `Umbraco.Tests.UnitTests`
- `Umbraco.Tests.Integration`
- `Umbraco.Tests.Benchmarks`

Internal types are accessible in test projects for more thorough testing.

## Architecture Principles

1. **Separation of Concerns**: Core defines contracts, Infrastructure implements them
2. **Interface-First**: Always define interfaces before implementations
3. **Notification Pattern**: Use notifications instead of events for extensibility
4. **Attempt Pattern**: Return typed results with operation status
5. **Composer Pattern**: Use composers for DI registration and configuration
6. **Scoping**: Use scopes for unit of work and transaction management
7. **No Direct Database Access**: Core has no repositories implementations
8. **Culture Variance**: Full support for multi-language content
9. **Extensibility**: Everything is designed to be extended or replaced

## Common Gotchas

1. **Don't implement repositories in Core** - They belong in Infrastructure
2. **Always use scopes** - Database operations require a scope
3. **Complete scopes** - Forgot `scope.Complete()`? Changes won't save
4. **Notification timing** - *Saving notifications are cancellable, *Saved are not
5. **Service locator** - Don't use `IServiceProvider` directly, use DI
6. **Culture handling** - Many operations require explicit culture parameter
7. **Published vs Draft** - `IContent` is draft, `IPublishedContent` is published
8. **Constants** - Use constants instead of magic strings (property editor aliases, etc.)

## Navigation Tips

- **Finding a service**: Look in `/Services` for interfaces
- **Finding models**: Check `/Models` and subdirectories by domain
- **Finding notifications**: Browse `/Notifications` for available events
- **Finding configuration**: Check `/Configuration/Models`
- **Finding constants**: Search `Constants-*.cs` files
- **Understanding operation results**: Check `/Services/OperationStatus`

## Further Resources

- Implementation details are in **Umbraco.Infrastructure**
- Web functionality is in **Umbraco.Web.Common**
- API endpoints are in **Umbraco.Cms.Api.*** projects
- Official docs: https://docs.umbraco.com/

---

**Remember**: Umbraco.Core is about **defining what**, not **implementing how**. Keep implementations in Infrastructure!
