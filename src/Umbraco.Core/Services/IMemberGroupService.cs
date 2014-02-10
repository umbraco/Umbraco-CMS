using System.Collections.Generic;
using Umbraco.Core.Models;

namespace Umbraco.Core.Services
{
    public interface IMemberGroupService : IService
    {
        IEnumerable<IMemberGroup> GetAll(params int[] ids);
        IMemberGroup Get(int id);
        void Save(IMemberGroup memberGroup, int userId = 0);
        void Delete(IMemberType memberType, int userId = 0);
    }
}