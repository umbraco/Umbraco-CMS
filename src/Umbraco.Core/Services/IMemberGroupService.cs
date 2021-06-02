using System;
using System.Collections.Generic;
using Umbraco.Core.Models;

namespace Umbraco.Core.Services
{
    public interface IMemberGroupService : IService
    {
        IMemberGroup GetById(int id);
        IMemberGroup GetById(Guid id);
        IEnumerable<IMemberGroup> GetAll();
        IEnumerable<IMemberGroup> GetByIds(IEnumerable<int> ids);
        IEnumerable<IMemberGroup> GetByIds(IEnumerable<Guid> ids);
        IMemberGroup GetByName(string name);
        void Save(IMemberGroup memberGroup, bool raiseEvents = true);
        void Delete(IMemberGroup memberGroup);
    }
}
