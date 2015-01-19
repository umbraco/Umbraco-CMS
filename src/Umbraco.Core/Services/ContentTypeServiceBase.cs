using System.Collections.Generic;
using System.Linq;
using Umbraco.Core.Logging;
using Umbraco.Core.Models;
using Umbraco.Core.Models.EntityBase;
using Umbraco.Core.Persistence;
using Umbraco.Core.Persistence.UnitOfWork;

namespace Umbraco.Core.Services
{
    public class ContentTypeServiceBase : RepositoryService
    {
        public ContentTypeServiceBase(IDatabaseUnitOfWorkProvider provider, RepositoryFactory repositoryFactory, ILogger logger)
            : base(provider, repositoryFactory, logger)
        {
        }

        /// <summary>
        /// This is called after an content type is saved and is used to update the content xml structures in the database
        /// if they are required to be updated.
        /// </summary>
        /// <param name="contentTypes"></param>
        internal IEnumerable<IContentTypeBase> GetContentTypesForXmlUpdates(params IContentTypeBase[] contentTypes)
        {

            var toUpdate = new List<IContentTypeBase>();

            foreach (var contentType in contentTypes)
            {
                //we need to determine if we need to refresh the xml content in the database. This is to be done when:
                // - the item is not new (already existed in the db) AND
                //      - a content type changes it's alias OR
                //      - if a content type has it's property removed OR
                //      - if a content type has a property whose alias has changed
                //here we need to check if the alias of the content type changed or if one of the properties was removed.                    
                var dirty = contentType as IRememberBeingDirty;
                if (dirty == null) continue;

                //check if any property types have changed their aliases (and not new property types)
                var hasAnyPropertiesChangedAlias = contentType.PropertyTypes.Any(propType =>
                    {
                        var dirtyProperty = propType as IRememberBeingDirty;
                        if (dirtyProperty == null) return false;
                        return dirtyProperty.WasPropertyDirty("HasIdentity") == false   //ensure it's not 'new'
                               && dirtyProperty.WasPropertyDirty("Alias");              //alias has changed
                    });

                if (dirty.WasPropertyDirty("HasIdentity") == false //ensure it's not 'new'
                    && (dirty.WasPropertyDirty("Alias") || dirty.WasPropertyDirty("HasPropertyTypeBeenRemoved") || hasAnyPropertiesChangedAlias))
                {
                    //If the alias was changed then we only need to update the xml structures for content of the current content type.
                    //If a property was deleted or a property alias was changed then we need to update the xml structures for any 
                    // content of the current content type and any of the content type's child content types.
                    if (dirty.WasPropertyDirty("Alias")
                        && dirty.WasPropertyDirty("HasPropertyTypeBeenRemoved") == false && hasAnyPropertiesChangedAlias == false)
                    {
                        //if only the alias changed then only update the current content type                        
                        toUpdate.Add(contentType);
                    }
                    else
                    {
                        //if a property was deleted or alias changed, then update all content of the current content type
                        // and all of it's desscendant doc types.     
                        toUpdate.AddRange(contentType.DescendantsAndSelf());
                    }
                }
            }

            return toUpdate;

        }
    }
}