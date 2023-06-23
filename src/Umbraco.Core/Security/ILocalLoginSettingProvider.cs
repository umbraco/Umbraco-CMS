namespace Umbraco.Cms.Core.Security;

/// <summary>
/// A setting provider local logins.
/// <remarks>
/// This cannot be an app setting since it's specified the external login providers.
/// </remarks>
/// </summary>
public interface ILocalLoginSettingProvider
{
    bool HasDenyLocalLogin();
}
