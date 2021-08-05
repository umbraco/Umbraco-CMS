using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Infrastructure.Persistence.Dtos;

namespace Umbraco.Cms.Infrastructure.Persistence.Factories
{
    internal static class TestFactory
    {
        public static ITest BuildEntity(TestDto dto)
        {
            var entity = new Test(dto.Id, dto.Name);
            // reset dirty initial properties (U4-1946)
            entity.ResetDirtyProperties(false);
            return entity;
        }

        public static TestDto BuildDto(ITest entity) =>
            new TestDto
            {
                Id = entity.Id,
                Name = entity.Name,
            };
    }
}
