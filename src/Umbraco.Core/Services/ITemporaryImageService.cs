using Umbraco.Cms.Core.Models;

namespace Umbraco.Cms.Core.Services;

public interface ITemporaryImageService
{
    public IMedia Save(string temporaryLocation);
}
