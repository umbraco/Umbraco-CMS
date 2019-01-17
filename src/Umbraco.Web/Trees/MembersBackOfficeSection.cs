using Umbraco.Core;
using Umbraco.Web.Models.ContentEditing;
using Umbraco.Web.Models.Trees;

namespace Umbraco.Web.Trees
{
    /// <summary>
    /// Defines the back office members section
    /// </summary>
    public class MembersBackOfficeSection : IBackOfficeSection
    {
        public string Alias => Constants.Applications.Members;
        public string Name => "Members";
    }
}