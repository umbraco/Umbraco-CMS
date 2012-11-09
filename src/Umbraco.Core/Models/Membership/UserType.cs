using Umbraco.Core.Models.EntityBase;

namespace Umbraco.Core.Models.Membership
{
    /// <summary>
    /// Represents the Type for a Backoffice User
    /// </summary>
    /// <remarks>
    /// Should be internal until a proper user/membership implementation
    /// is part of the roadmap.
    /// </remarks>
    internal class UserType : Entity, IUserType
    {
        public string Alias { get; set; }
        public string Name { get; set; }
        public string Permissions { get; set; }
    }
}