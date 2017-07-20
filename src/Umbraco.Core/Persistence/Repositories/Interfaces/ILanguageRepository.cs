using Umbraco.Core.Models;

namespace Umbraco.Core.Persistence.Repositories
{
    public interface ILanguageRepository : IQueryRepository<int, ILanguage>
    {
        ILanguage GetByCultureName(string cultureName);
        ILanguage GetByIsoCode(string isoCode);
    }
}
