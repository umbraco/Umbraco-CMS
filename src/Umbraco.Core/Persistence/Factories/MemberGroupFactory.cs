using System;
using Umbraco.Core.Models;
using Umbraco.Core.Models.Rdbms;

namespace Umbraco.Core.Persistence.Factories
{
    internal class MemberGroupFactory 
    {
        
        private readonly Guid _nodeObjectTypeId;

        public MemberGroupFactory()
        {
            _nodeObjectTypeId = new Guid(Constants.ObjectTypes.MemberGroup);
        }

        #region Implementation of IEntityFactory<ITemplate,TemplateDto>

        public IMemberGroup BuildEntity(NodeDto dto)
        {
            var group = new MemberGroup();
            
            try
            {
                group.DisableChangeTracking();

                group.CreateDate = dto.CreateDate;
                group.Id = dto.NodeId;
                group.Key = dto.UniqueId;
                group.Name = dto.Text;

                //on initial construction we don't want to have dirty properties tracked
                // http://issues.umbraco.org/issue/U4-1946
                group.ResetDirtyProperties(false);
                return group;
            }
            finally
            {
                group.EnableChangeTracking();
            }
        }

        public NodeDto BuildDto(IMemberGroup entity)
        {
            var dto = new NodeDto
            {
                CreateDate = entity.CreateDate,
                NodeId = entity.Id,
                Level = 0,
                NodeObjectType = _nodeObjectTypeId,
                ParentId = -1,
                Path = "",
                SortOrder = 0,
                Text = entity.Name,
                Trashed = false,
                UniqueId = entity.Key,
                UserId = entity.CreatorId
            };

            if (entity.HasIdentity)
            {
                dto.NodeId = entity.Id;
                dto.Path = "-1," + entity.Id;
            }

            return dto;
        }

        #endregion

    }
}