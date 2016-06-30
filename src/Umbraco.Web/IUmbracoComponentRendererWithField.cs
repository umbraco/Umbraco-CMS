using System.Web;
using Umbraco.Core.Models.PublishedContent;

namespace Umbraco.Web
{
    /// <summary>
    /// Methods used to render umbraco components as HTML in templates
    /// </summary>
    public interface IUmbracoComponentRendererWithField : IUmbracoComponentRenderer
    {
        /// <summary>
        /// Renders an field to the template
        /// </summary>
        /// <param name="currentPage"></param>
        /// <param name="fieldAlias"></param>
        /// <param name="altFieldAlias"></param>
        /// <param name="altText"></param>
        /// <param name="insertBefore"></param>
        /// <param name="insertAfter"></param>
        /// <param name="recursive"></param>
        /// <param name="convertLineBreaks"></param>
        /// <param name="removeParagraphTags"></param>
        /// <param name="casing"></param>
        /// <param name="encoding"></param>
        /// <param name="formatAsDate"></param>
        /// <param name="formatAsDateWithTime"></param>
        /// <param name="formatAsDateWithTimeSeparator"></param>
        //// <param name="formatString"></param>
        /// <returns></returns>
        IHtmlString Field(IPublishedContent currentPage, string fieldAlias, 
            string altFieldAlias = "", string altText = "", string insertBefore = "", string insertAfter = "",
            bool recursive = false, bool convertLineBreaks = false, bool removeParagraphTags = false,
            RenderFieldCaseType casing = RenderFieldCaseType.Unchanged,
            RenderFieldEncodingType encoding = RenderFieldEncodingType.Unchanged, 
            bool formatAsDate =  false,
            bool formatAsDateWithTime = false,
            string formatAsDateWithTimeSeparator = "");
    }
}