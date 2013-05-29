using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Umbraco.Core;
using Umbraco.Core.Models;
using Umbraco.Core.PropertyEditors;
using Umbraco.Web.Models.ContentEditing;

namespace Umbraco.Web.Models.Mapping
{
    //TODO: Convert this over to mapping engine if people agree to it
    internal class ContentModelMapper
    {
        private readonly ApplicationContext _applicationContext;

        public ContentModelMapper(ApplicationContext applicationContext)
        {
            _applicationContext = applicationContext;
        }

        public ContentItemDisplay ToContentItemDisplay(IContent content)
        {            
            return new ContentItemDisplay
                {
                    Id = content.Id,
                    Name = content.Name,
                    Properties = content.Properties.Select(p =>
                        {
                            var editor = PropertyEditorResolver.Current.GetById(p.PropertyType.DataTypeId);
                            if (editor == null)
                            {
                                throw new NullReferenceException("The property editor with id " + p.PropertyType.DataTypeId + " does not exist");
                            }
                            return new ContentPropertyDisplay
                                {
                                    Alias = p.Alias,
                                    Id = p.Id,
                                    Description = p.PropertyType.Description,
                                    Label = p.PropertyType.Name,
                                    Config = _applicationContext.Services.DataTypeService.GetPreValuesByDataTypeId(p.PropertyType.DataTypeDefinitionId),
                                    Value = editor.ValueEditor.SerializeValue(p.Value),
                                    View = editor.ValueEditor.View
                                };
                        }).ToArray()
                };
        }

        public ContentItemDto ToContentItemDto(IContent content)
        {
            return new ContentItemDto
                {
                    Id = content.Id,
                    Properties = content.Properties.Select(p => new ContentPropertyDto
                        {
                            Alias = p.Alias,
                            Description = p.PropertyType.Description,
                            Label = p.PropertyType.Name,
                            Id = p.Id,
                            DataType = _applicationContext.Services.DataTypeService.GetDataTypeDefinitionById(p.PropertyType.DataTypeDefinitionId)
                        }).ToList()
                };
        }

    }
}
