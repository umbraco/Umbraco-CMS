using System;
using System.Collections.Generic;
using Umbraco.Core.Models;

namespace Umbraco.Core.Services
{
    public interface IMemberGroupService : IService
    {
        IEnumerable<IMemberGroup> GetAll();
        IMemberGroup GetById(int id);
        IMemberGroup GetById(Guid id);
        IEnumerable<IMemberGroup> GetByIds(IEnumerable<int> ids);
        IMemberGroup GetByName(string name);
        void Save(IMemberGroup memberGroup, bool raiseEvents = true);
        void Delete(IMemberGroup memberGroup);
    }
}
