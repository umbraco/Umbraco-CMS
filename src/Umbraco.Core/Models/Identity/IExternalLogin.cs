namespace Umbraco.Core.Models.Identity
{
    /// <summary>
    /// Used to persist external login data for a user
    /// </summary>
    public interface IExternalLogin
    {
        string LoginProvider { get; }
        string ProviderKey { get; }
        string UserData { get; }
    }
}
