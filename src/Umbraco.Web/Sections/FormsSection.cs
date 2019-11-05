using Umbraco.Core;
using Umbraco.Core.Models.Sections;

namespace Umbraco.Web.Sections
{
    /// <summary>
    /// Defines the back office media section
    /// </summary>
    public class FormsSection : ISection
    {
        public string Alias => ConstantsCore.Applications.Forms;
        public string Name => "Forms";
    }
}
