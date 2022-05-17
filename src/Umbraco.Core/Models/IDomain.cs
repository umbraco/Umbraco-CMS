using Umbraco.Cms.Core.Models.Entities;

namespace Umbraco.Cms.Core.Models;

public interface IDomain : IEntity, IRememberBeingDirty
{
    int? LanguageId { get; set; }

    string DomainName { get; set; }

    int? RootContentId { get; set; }

    bool IsWildcard { get; }

    /// <summary>
    ///     Readonly value of the language ISO code for the domain
    /// </summary>
    string? LanguageIsoCode { get; }
}
