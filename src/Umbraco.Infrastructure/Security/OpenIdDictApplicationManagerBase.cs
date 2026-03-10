using OpenIddict.Abstractions;

namespace Umbraco.Cms.Infrastructure.Security;

public abstract class OpenIdDictApplicationManagerBase
{
    protected IOpenIddictApplicationManager ApplicationManager { get; }

    protected OpenIdDictApplicationManagerBase(IOpenIddictApplicationManager applicationManager)
        => ApplicationManager = applicationManager;

    protected async Task CreateOrUpdate(OpenIddictApplicationDescriptor clientDescriptor, CancellationToken cancellationToken)
    {
        var identifier = clientDescriptor.ClientId ??
                         throw new ApplicationException($"ClientId is missing for application: {clientDescriptor.DisplayName ?? "(no name)"}");
        var client = await ApplicationManager.FindByClientIdAsync(identifier, cancellationToken);
        if (client is null)
        {
            await ApplicationManager.CreateAsync(clientDescriptor, cancellationToken);
        }
        else
        {
            await ApplicationManager.UpdateAsync(client, clientDescriptor, cancellationToken);
        }
    }

    protected async Task Delete(string identifier, CancellationToken cancellationToken)
    {
        var client = await ApplicationManager.FindByClientIdAsync(identifier, cancellationToken);
        if (client is null)
        {
            return;
        }

        await ApplicationManager.DeleteAsync(client, cancellationToken);
    }
}
