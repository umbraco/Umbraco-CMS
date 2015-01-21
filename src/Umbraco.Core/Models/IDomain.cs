using Umbraco.Core.Models.EntityBase;

namespace Umbraco.Core.Models
{
    public interface IDomain : IAggregateRoot, IRememberBeingDirty, ICanBeDirty
    {
        ILanguage Language { get; set; }
        string DomainName { get; set; }
        IContent RootContent { get; set; }
        bool IsWildcard { get; }
    }
}