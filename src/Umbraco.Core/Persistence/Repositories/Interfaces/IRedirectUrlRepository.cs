using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Umbraco.Core.Models;

namespace Umbraco.Core.Persistence.Repositories
{
    interface IRedirectUrlRepository : IRepositoryQueryable<int, IRedirectUrl>
    {
    }
}
