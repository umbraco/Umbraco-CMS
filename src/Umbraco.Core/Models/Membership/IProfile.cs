namespace Umbraco.Core.Models.Membership
{
    /// <summary>
    /// Defines the the User Profile interface
    /// </summary>
    public interface IProfile
    {
        int Id { get; }
        string Name { get; }
    }
}
