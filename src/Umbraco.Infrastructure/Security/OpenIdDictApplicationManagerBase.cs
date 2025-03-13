using OpenIddict.Abstractions;

namespace Umbraco.Cms.Infrastructure.Security;

public abstract class OpenIdDictApplicationManagerBase
{
    private readonly IOpenIddictApplicationManager _applicationManager;

    protected OpenIdDictApplicationManagerBase(IOpenIddictApplicationManager applicationManager)
        => _applicationManager = applicationManager;

    protected async Task CreateOrUpdate(OpenIddictApplicationDescriptor clientDescriptor, CancellationToken cancellationToken)
    {
        var identifier = clientDescriptor.ClientId ??
                         throw new ApplicationException($"ClientId is missing for application: {clientDescriptor.DisplayName ?? "(no name)"}");
        var client = await _applicationManager.FindByClientIdAsync(identifier, cancellationToken);
        if (client is null)
        {
            await _applicationManager.CreateAsync(clientDescriptor, cancellationToken);
        }
        else
        {
            await _applicationManager.UpdateAsync(client, clientDescriptor, cancellationToken);
        }
    }

    protected async Task Delete(string identifier, CancellationToken cancellationToken)
    {
        var client = await _applicationManager.FindByClientIdAsync(identifier, cancellationToken);
        if (client is null)
        {
            return;
        }

        await _applicationManager.DeleteAsync(client, cancellationToken);
    }
}
