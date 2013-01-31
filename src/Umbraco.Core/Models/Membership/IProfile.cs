namespace Umbraco.Core.Models.Membership
{
    /// <summary>
    /// Defines the the Profile interface
    /// </summary>
    public interface IProfile
    {
        object Id { get; set; }
        string Name { get; set; }
    }
}