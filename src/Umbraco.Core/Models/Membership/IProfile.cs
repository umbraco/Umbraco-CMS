namespace Umbraco.Core.Models.Membership
{
    /// <summary>
    /// Defines the the Profile interface
    /// </summary>
    /// <remarks>
    /// This interface is pretty useless but has been exposed publicly from 6.x so we're stuck with it. It would make more sense 
    /// if the Id was an int but since it's not people have to cast it to int all of the time!
    /// </remarks>
    public interface IProfile
    {
        object Id { get; set; }
        string Name { get; set; }
    }
}