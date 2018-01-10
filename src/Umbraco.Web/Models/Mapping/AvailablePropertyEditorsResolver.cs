using System.Collections.Generic;
using System.Linq;
using AutoMapper;
using Umbraco.Core.Configuration.UmbracoSettings;
using Umbraco.Core.Models;
using Umbraco.Web.Composing;
using Umbraco.Web.Models.ContentEditing;

namespace Umbraco.Web.Models.Mapping
{
    internal class AvailablePropertyEditorsResolver
    {
        private readonly IContentSection _contentSection;

        public AvailablePropertyEditorsResolver(IContentSection contentSection)
        {
            _contentSection = contentSection;
        }

        public IEnumerable<PropertyEditorBasic> Resolve(IDataType source)
        {
            return Current.PropertyEditors
                .Where(x =>
                {
                    // fixme should we support deprecating?
                    //if (_contentSection.ShowDeprecatedPropertyEditors)
                    //    return true;
                    return source.EditorAlias == x.Alias || x.IsDeprecated == false;
                })
                .OrderBy(x => x.Name)
                .Select(Mapper.Map<PropertyEditorBasic>);
        }
    }
}
