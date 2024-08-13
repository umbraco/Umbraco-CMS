using Umbraco.Cms.Core.Services.OperationStatus;

namespace Umbraco.Cms.Core.Services;

public interface IOEmbedService
{
    Task<Attempt<string, OEmbedOperationStatus>> GetMarkupAsync(Uri url, int? width, int? height, CancellationToken cancellationToken);
}
