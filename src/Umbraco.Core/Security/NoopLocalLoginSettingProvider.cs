namespace Umbraco.Cms.Core.Security;

/// <summary>
///     A default implementation of <see cref="ILocalLoginSettingProvider"/> that always allows local login.
/// </summary>
/// <remarks>
///     This is used when the backoffice is not registered, as the backoffice provides its own implementation
///     that checks external login provider settings.
/// </remarks>
public sealed class NoopLocalLoginSettingProvider : ILocalLoginSettingProvider
{
    /// <inheritdoc />
    public bool HasDenyLocalLogin() => false;
}
