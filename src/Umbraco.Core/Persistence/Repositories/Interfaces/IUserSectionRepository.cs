using System;
using Umbraco.Core.Models;

namespace Umbraco.Core.Persistence.Repositories
{
    internal interface IUserSectionRepository : IRepository<Tuple<int, string>, UserSection>
    {
        
    }
}