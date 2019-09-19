using System;
using Umbraco.Core.Configuration.UmbracoSettings;
using Umbraco.Core.Logging;
using Umbraco.Core.Persistence.SqlSyntax;
using Umbraco.Core.Persistence.UnitOfWork;

namespace Umbraco.Core.Persistence.Repositories
{
    /// <summary>
    /// Override the base content repository so we can change the node object type
    /// </summary>
    /// <remarks>
    /// It would be nicer if we could separate most of this down into a smaller version of the ContentRepository class, however to do that 
    /// requires quite a lot of work since we'd need to re-organize the interhitance quite a lot or create a helper class to perform a lot of the underlying logic.
    /// 
    /// TODO: Create a helper method to contain most of the underlying logic for the ContentRepository
    /// </remarks>
    internal class ContentBlueprintRepository : ContentRepository
    {
        public ContentBlueprintRepository(IScopeUnitOfWork work, CacheHelper cacheHelper, ILogger logger, ISqlSyntaxProvider syntaxProvider, IContentTypeRepository contentTypeRepository, ITemplateRepository templateRepository, ITagRepository tagRepository, IContentSection contentSection) 
            : base(work, cacheHelper, logger, syntaxProvider, contentTypeRepository, templateRepository, tagRepository, contentSection)
        {
        }

        protected override Guid NodeObjectTypeId
        {
            get { return Constants.ObjectTypes.DocumentBlueprintGuid; }
        }
        
    }
}