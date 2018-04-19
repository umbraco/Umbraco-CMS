using Umbraco.Core.Models;

namespace Umbraco.Core.Persistence.Repositories
{
    public interface ILanguageRepository : IReadWriteQueryRepository<int, ILanguage>
    {
        ILanguage GetByCultureName(string cultureName);
        ILanguage GetByIsoCode(string isoCode);

        int GetIdByIsoCode(string isoCode);
        string GetIsoCodeById(int id);
    }
}
