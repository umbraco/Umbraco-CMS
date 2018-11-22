namespace Umbraco.Core.Models.Membership
{

    /// <summary>
    /// How membership is implemented in the current install.
    /// </summary>
    public enum MembershipScenario
    {
        /// <summary>
        /// The member is based on the native Umbraco members (IMember + Umbraco membership provider)
        /// </summary>
        /// <remarks>
        /// This supports custom member properties
        /// </remarks>
        NativeUmbraco,

        /// <summary>
        /// The member is based on a custom member provider but it is linked to an IMember
        /// </summary>
        /// <remarks>
        /// This supports custom member properties (but that is not enabled yet)
        /// </remarks>
        CustomProviderWithUmbracoLink,

        /// <summary>
        /// The member is based purely on a custom member provider and is not linked to umbraco data
        /// </summary>
        /// <remarks>
        /// This does not support custom member properties.
        /// </remarks>
        StandaloneCustomProvider
    }
}