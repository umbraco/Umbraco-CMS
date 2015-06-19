using System;
using Umbraco.Core.Models;
using Umbraco.Core.Models.Rdbms;

namespace Umbraco.Core.Persistence.Factories
{
    internal class MigrationEntryFactory
    {
        public MigrationEntry BuildEntity(MigrationDto dto)
        {
            var model = new MigrationEntry(dto.Id, dto.CreateDate, dto.Name, Version.Parse(dto.Version));
            //on initial construction we don't want to have dirty properties tracked
            // http://issues.umbraco.org/issue/U4-1946
            model.ResetDirtyProperties(false);
            return model;
        }

        public MigrationDto BuildDto(IMigrationEntry entity)
        {
            var dto = new MigrationDto
            {
                CreateDate = entity.CreateDate,
                Name = entity.MigrationName,
                Version = entity.Version.ToString()
            };

            if (entity.HasIdentity)
                dto.Id = entity.Id;

            return dto;
        }
    }
}