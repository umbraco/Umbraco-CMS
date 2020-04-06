namespace Umbraco.Core.Services
{
    /// <summary>
    /// Placeholder for sharing logic between the content, media (and member) services
    /// TODO: Start sharing the logic!
    /// </summary>
    public interface IContentServiceBase : IService
    {

        /// <summary>
        /// Checks the data integrity of the node paths/levels stored in the database
        /// </summary>
        bool VerifyNodePaths(out int[] invalidIds);

        /// <summary>
        /// Fixes the data integrity of node paths/levels stored in the database
        /// </summary>
        void FixNodePaths();
    }
}
