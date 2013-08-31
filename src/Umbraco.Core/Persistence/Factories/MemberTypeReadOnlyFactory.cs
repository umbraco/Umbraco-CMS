using Umbraco.Core.Models;
using Umbraco.Core.Models.Rdbms;

namespace Umbraco.Core.Persistence.Factories
{
    internal class MemberTypeReadOnlyFactory : IEntityFactory<IMemberType, MemberTypeReadOnlyDto>
    {
        public IMemberType BuildEntity(MemberTypeReadOnlyDto dto)
        {
            throw new System.NotImplementedException();
        }

        public MemberTypeReadOnlyDto BuildDto(IMemberType entity)
        {
            throw new System.NotImplementedException();
        }
    }
}