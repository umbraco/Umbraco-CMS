using System.Collections.Generic;
using Umbraco.Core.Models;

namespace Umbraco.Core.Persistence.Repositories
{
    public interface IStylesheetRepository : IRepository<string, Stylesheet>
    {
        bool ValidateStylesheet(Stylesheet stylesheet);
    }
}