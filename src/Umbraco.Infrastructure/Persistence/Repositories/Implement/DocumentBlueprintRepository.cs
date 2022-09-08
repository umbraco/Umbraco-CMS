using Microsoft.Extensions.Logging;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Persistence.Repositories;
using Umbraco.Cms.Core.PropertyEditors;
using Umbraco.Cms.Core.Serialization;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Infrastructure.Scoping;

namespace Umbraco.Cms.Infrastructure.Persistence.Repositories.Implement;

/// <summary>
///     Override the base content repository so we can change the node object type
/// </summary>
/// <remarks>
///     It would be nicer if we could separate most of this down into a smaller version of the ContentRepository class,
///     however to do that
///     requires quite a lot of work since we'd need to re-organize the inheritance quite a lot or create a helper class to
///     perform a lot of the underlying logic.
///     TODO: Create a helper method to contain most of the underlying logic for the ContentRepository
/// </remarks>
internal class DocumentBlueprintRepository : DocumentRepository, IDocumentBlueprintRepository
{
    public DocumentBlueprintRepository(
        IScopeAccessor scopeAccessor,
        AppCaches appCaches,
        ILogger<DocumentBlueprintRepository> logger,
        ILoggerFactory loggerFactory,
        IContentTypeRepository contentTypeRepository,
        ITemplateRepository templateRepository,
        ITagRepository tagRepository,
        ILanguageRepository languageRepository,
        IRelationRepository relationRepository,
        IRelationTypeRepository relationTypeRepository,
        PropertyEditorCollection propertyEditorCollection,
        IDataTypeService dataTypeService,
        DataValueReferenceFactoryCollection dataValueReferenceFactories,
        IJsonSerializer serializer,
        IEventAggregator eventAggregator)
        : base(
            scopeAccessor,
            appCaches,
            logger,
            loggerFactory,
            contentTypeRepository,
            templateRepository,
            tagRepository,
            languageRepository,
            relationRepository,
            relationTypeRepository,
            propertyEditorCollection,
            dataValueReferenceFactories,
            dataTypeService,
            serializer,
            eventAggregator)
    {
    }

    protected override bool EnsureUniqueNaming => false; // duplicates are allowed

    protected override Guid NodeObjectTypeId => Constants.ObjectTypes.DocumentBlueprint;
}
