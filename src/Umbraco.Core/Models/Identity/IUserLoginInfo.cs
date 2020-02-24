namespace Umbraco.Core.Models.Identity
{
    public interface IUserLoginInfo
    {
        /// <summary>
        ///     Provider for the linked login, i.e. Facebook, Google, etc.
        /// </summary>
        string LoginProvider { get; set; }

        /// <summary>User specific key for the login provider</summary>
        string ProviderKey { get; set; }
    }
}
