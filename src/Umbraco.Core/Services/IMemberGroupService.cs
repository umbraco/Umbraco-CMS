using System.Collections.Generic;
using Umbraco.Core.Models;

namespace Umbraco.Core.Services
{
    public interface IMemberGroupService : IService
    {
        IEnumerable<IMemberGroup> GetAll();
        IMemberGroup GetById(int id);
        IMemberGroup GetByName(string name);
        void Save(IMemberGroup memberGroup, bool raiseEvents = true);
        void Delete(IMemberGroup memberGroup);
    }
}