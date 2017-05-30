using System.Collections.Generic;
using System.Linq;
using AutoMapper;
using Umbraco.Core.Configuration.UmbracoSettings;
using Umbraco.Core.Models;
using Umbraco.Web.Composing;
using Umbraco.Web.Models.ContentEditing;

namespace Umbraco.Web.Models.Mapping
{
    internal class AvailablePropertyEditorsResolver : ValueResolver<IDataTypeDefinition, IEnumerable<PropertyEditorBasic>>
    {
        private readonly IContentSection _contentSection;

        public AvailablePropertyEditorsResolver(IContentSection contentSection)
        {
            _contentSection = contentSection;
        }

        protected override IEnumerable<PropertyEditorBasic> ResolveCore(IDataTypeDefinition source)
        {
            return Current.PropertyEditors
                .Where(x =>
                {                    
                    // fixme should we support deprecating?
                    //if (_contentSection.ShowDeprecatedPropertyEditors)
                    //    return true;
                    return source.PropertyEditorAlias == x.Alias || x.IsDeprecated == false;
                })
                .OrderBy(x => x.Name)
                .Select(Mapper.Map<PropertyEditorBasic>);
        }
    }
}