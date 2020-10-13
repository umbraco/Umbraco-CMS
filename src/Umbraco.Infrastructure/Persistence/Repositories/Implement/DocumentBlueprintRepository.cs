using System;
using Microsoft.Extensions.Logging;
using Umbraco.Core.Cache;
using Umbraco.Core.Configuration.UmbracoSettings;
using Umbraco.Core.PropertyEditors;
using Umbraco.Core.Scoping;
using Umbraco.Core.Services;

namespace Umbraco.Core.Persistence.Repositories.Implement
{
    /// <summary>
    /// Override the base content repository so we can change the node object type
    /// </summary>
    /// <remarks>
    /// It would be nicer if we could separate most of this down into a smaller version of the ContentRepository class, however to do that
    /// requires quite a lot of work since we'd need to re-organize the inheritance quite a lot or create a helper class to perform a lot of the underlying logic.
    ///
    /// TODO: Create a helper method to contain most of the underlying logic for the ContentRepository
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
            Lazy<PropertyEditorCollection> propertyEditorCollection,
            IDataTypeService dataTypeService,
            DataValueReferenceFactoryCollection dataValueReferenceFactories)
            : base(scopeAccessor, appCaches, logger, loggerFactory, contentTypeRepository, templateRepository, tagRepository, languageRepository, relationRepository, relationTypeRepository, propertyEditorCollection, dataValueReferenceFactories, dataTypeService)
        {
        }

        protected override bool EnsureUniqueNaming => false; // duplicates are allowed

        protected override Guid NodeObjectTypeId => Constants.ObjectTypes.DocumentBlueprint;
    }
}
