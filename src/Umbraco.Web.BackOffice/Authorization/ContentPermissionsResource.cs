using System.Collections.Generic;
using Umbraco.Core.Models;

namespace Umbraco.Web.BackOffice.Authorization
{
    /// <summary>
    /// The resource used for the <see cref="ContentPermissionsResourceRequirement"/>
    /// </summary>
    public class ContentPermissionsResource
    {
        public ContentPermissionsResource(IContent content, char permissionToCheck)
        {
            PermissionsToCheck = new List<char> { permissionToCheck };
            Content = content;
        }

        public ContentPermissionsResource(IContent content, IReadOnlyList<char> permissionToCheck)
        {
            Content = content;
            PermissionsToCheck = permissionToCheck;
        }

        public ContentPermissionsResource(IContent content, int nodeId, IReadOnlyList<char> permissionToCheck)
        {
            Content = content;
            NodeId = nodeId;
            PermissionsToCheck = permissionToCheck;
        }

        public int? NodeId { get; } 
        public IReadOnlyList<char> PermissionsToCheck { get; }
        public IContent Content { get; }
    }
}
