using Umbraco.Core;
using Umbraco.Core.Models.Trees;

namespace Umbraco.Web.Trees
{
    /// <summary>
    /// Defines the back office media section
    /// </summary>
    public class FormsBackOfficeSection : IBackOfficeSection
    {
        public string Alias => Constants.Applications.Forms;
        public string Name => "Forms";
    }
}
