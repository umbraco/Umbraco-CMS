namespace Umbraco.Core.Configuration.Dashboard
{
    internal class AccessItem : IAccessItem
    {
        /// <summary>
        /// This can be grant, deny or grantBySection
        /// </summary>
        public AccessType Action { get; set; }
        
        /// <summary>
        /// The value of the action
        /// </summary>
        public string Value { get; set; }
    }
}