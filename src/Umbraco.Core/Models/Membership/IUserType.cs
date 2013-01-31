using Umbraco.Core.Models.EntityBase;

namespace Umbraco.Core.Models.Membership
{
    internal interface IUserType : IAggregateRoot
    {
        string Alias { get; set; }
        string Name { get; set; }
        string Permissions { get; set; }
    }
}