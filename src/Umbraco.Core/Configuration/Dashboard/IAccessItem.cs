namespace Umbraco.Core.Configuration.Dashboard
{
    public interface IAccessItem
    {
        /// <summary>
        /// This can be grant, deny or grantBySection
        /// </summary>
        AccessType Action { get; set; }

        /// <summary>
        /// The value of the action
        /// </summary>
        string Value { get; set; }
    }
}