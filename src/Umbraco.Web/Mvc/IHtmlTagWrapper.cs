using System.Web.UI;

namespace Umbraco.Web.Mvc
{
    public interface IHtmlTagWrapper
    {
        void WriteToHtmlTextWriter(HtmlTextWriter html);
    }
}
