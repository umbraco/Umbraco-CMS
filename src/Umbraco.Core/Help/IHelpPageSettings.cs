namespace Umbraco.Core.Help
{
    public interface IHelpPageSettings
    {
        /// <summary>
        /// Gets the allowed addresses to retrieve data for the help page.
        /// </summary>
        string HelpPageUrlAllowList { get; }
    }
}
