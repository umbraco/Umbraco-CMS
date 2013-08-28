namespace Umbraco.Core.Models.Membership
{
    internal interface IMember : IMembershipUser, IMemberProfile
    {
        new int Id { get; set; }
    }

    internal interface IMemberProfile : IProfile
    {

    }
}