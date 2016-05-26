namespace Umbraco.Web.PublishedCache
{
    /// <summary>
    /// The Umbraco facade.
    /// </summary>
    public interface IFacade
    {
        /// <summary>
        /// Gets the <see cref="IPublishedContentCache"/>.
        /// </summary>
        IPublishedContentCache ContentCache { get; }

        /// <summary>
        /// Gets the <see cref="IPublishedMediaCache"/>.
        /// </summary>
        IPublishedMediaCache MediaCache { get; }

        /// <summary>
        /// Gets the <see cref="IPublishedMemberCache"/>.
        /// </summary>
        IPublishedMemberCache MemberCache { get; }

        /// <summary>
        /// Gets the <see cref="IDomainCache"/>.
        /// </summary>
        IDomainCache DomainCache { get; }
    }
}
