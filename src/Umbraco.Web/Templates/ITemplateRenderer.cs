using System.IO;

namespace Umbraco.Web.Templates
{
    /// <summary>
    /// This is used purely for the RenderTemplate functionality in Umbraco
    /// </summary>
    public interface ITemplateRenderer
    {
        void Render(int pageId, int? altTemplateId, StringWriter writer);
    }
}
