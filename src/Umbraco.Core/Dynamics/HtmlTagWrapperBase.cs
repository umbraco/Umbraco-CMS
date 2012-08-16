using System.Web.UI;

namespace Umbraco.Core.Dynamics
{
    internal interface IHtmlTagWrapper
    {
        void WriteToHtmlTextWriter(HtmlTextWriter html);
    }
}
