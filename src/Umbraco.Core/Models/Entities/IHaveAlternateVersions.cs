namespace Umbraco.Cms.Core.Models.Entities
{
    public interface IHaveAlternateVersions
    {
        /// <summary>
        ///     Gets the bool value indicating this is not the current draft version.
        /// </summary>
        bool IsAlternateVersion { get; set; }
    }
}
