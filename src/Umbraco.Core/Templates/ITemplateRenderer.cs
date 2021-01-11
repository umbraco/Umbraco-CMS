using System.IO;
using System.Threading.Tasks;

namespace Umbraco.Web.Templates
{
    /// <summary>
    /// This is used purely for the RenderTemplate functionality in Umbraco
    /// </summary>
    public interface ITemplateRenderer
    {
        Task RenderAsync(int pageId, int? altTemplateId, StringWriter writer);
    }
}
