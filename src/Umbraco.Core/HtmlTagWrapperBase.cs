using System.Web.UI;

namespace Umbraco.Core
{
    internal interface IHtmlTagWrapper
    {
        void WriteToHtmlTextWriter(HtmlTextWriter html);
    }
}
