using Umbraco.Core.Models;

namespace Umbraco.Core.Persistence.Repositories
{
    public interface ILanguageRepository : IRepositoryQueryable<int, ILanguage>
    {
        ILanguage GetByCultureName(string cultureName);
        ILanguage GetByIsoCode(string isoCode);
    }
}