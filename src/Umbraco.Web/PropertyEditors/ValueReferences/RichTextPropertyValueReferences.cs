using System.Collections.Generic;
using Umbraco.Core;
using Umbraco.Core.Models.Editors;
using Umbraco.Core.PropertyEditors;
using Umbraco.Web.Templates;

namespace Umbraco.Web.PropertyEditors.ValueReferences
{
    public class RichTextPropertyValueReferences : IDataValueReference
    {
        private HtmlImageSourceParser _imageSourceParser;
        private HtmlLocalLinkParser _localLinkParser;

        public RichTextPropertyValueReferences(HtmlImageSourceParser imageSourceParser, HtmlLocalLinkParser localLinkParser)
        {
            _imageSourceParser = imageSourceParser;
            _localLinkParser = localLinkParser;
        }

        /// <summary>
        /// Resolve references from <see cref="IDataValueEditor"/> values
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public IEnumerable<UmbracoEntityReference> GetReferences(object value)
        {
            var asString = value == null ? string.Empty : value is string str ? str : value.ToString();

            foreach (var udi in _imageSourceParser.FindUdisFromDataAttributes(asString))
                yield return new UmbracoEntityReference(udi);

            foreach (var udi in _localLinkParser.FindUdisFromLocalLinks(asString))
                yield return new UmbracoEntityReference(udi);

            //TODO: Detect Macros too ... but we can save that for a later date, right now need to do media refs
        }

        public bool IsForEditor(IDataEditor dataEditor) => dataEditor.Alias.InvariantEquals(Constants.PropertyEditors.Aliases.TinyMce);
    }
}
