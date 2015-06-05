using System;
using System.Linq;

using AutoMapper;
using Umbraco.Core;
using Umbraco.Core.Models;
using Umbraco.Core.Models.Mapping;
using Umbraco.Core.PropertyEditors;
using Umbraco.Web.Models.ContentEditing;
using System.Collections.Generic;

namespace Umbraco.Web.Models.Mapping
{
    /// <summary>
    /// Defines mappings for content/media/members type mappings
    /// </summary>
    internal class MemberTypeModelMapper : MapperConfiguration
    {
        private readonly Lazy<PropertyEditorResolver> _propertyEditorResolver;
        
        //default ctor
        public MemberTypeModelMapper()
        {
            _propertyEditorResolver = new Lazy<PropertyEditorResolver>(() => PropertyEditorResolver.Current);
        }

        //ctor can be used for testing
        public MemberTypeModelMapper(Lazy<PropertyEditorResolver> propertyEditorResolver)
        {
            _propertyEditorResolver = propertyEditorResolver;
        }

        public override void ConfigureMappings(IConfiguration config, ApplicationContext applicationContext)
        {
            config.CreateMap<IMemberType, ContentTypeBasic>();
            config.CreateMap<IMemberType, MemberTypeDisplay>()
                //Ignore because this is not actually used for content types
                .ForMember(display => display.Trashed, expression => expression.Ignore())
                .ForMember(
                    dto => dto.Groups,
                    expression => expression.ResolveUsing(new PropertyTypeGroupResolver(applicationContext, _propertyEditorResolver)));
        }
    }
}
