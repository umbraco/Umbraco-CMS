using AutoMapper;
using Umbraco.Core;
using Umbraco.Core.Models;
using Umbraco.Core.Models.Mapping;
using Umbraco.Web.Models.ContentEditing;
using umbraco;

namespace Umbraco.Web.Models.Mapping
{
    /// <summary>
    /// Declares model mappings for members.
    /// </summary>
    internal class MemberModelMapper : MapperConfiguration
    {
        public override void ConfigureMappings(IConfiguration config, ApplicationContext applicationContext)
        {
            //FROM IMember TO MediaItemDisplay
            config.CreateMap<IMember, MemberDisplay>()
                  .ForMember(
                      dto => dto.Owner,
                      expression => expression.ResolveUsing<OwnerResolver<IMember>>())
                  .ForMember(
                      dto => dto.Icon,
                      expression => expression.MapFrom(content => content.ContentType.Icon))
                  .ForMember(
                      dto => dto.ContentTypeAlias,
                      expression => expression.MapFrom(content => content.ContentType.Alias))
                  .ForMember(
                      dto => dto.ContentTypeName,
                      expression => expression.MapFrom(content => content.ContentType.Name))
                  .ForMember(display => display.Properties, expression => expression.Ignore())
                  .ForMember(display => display.Tabs, expression => expression.ResolveUsing<TabsAndPropertiesResolver>())
                  .AfterMap(MapGenericCustomProperties);

            //FROM IMember TO ContentItemBasic<ContentPropertyBasic, IMember>
            config.CreateMap<IMember, ContentItemBasic<ContentPropertyBasic, IMember>>()
                  .ForMember(
                      dto => dto.Owner,
                      expression => expression.ResolveUsing<OwnerResolver<IMember>>())
                  .ForMember(
                      dto => dto.Icon,
                      expression => expression.MapFrom(content => content.ContentType.Icon))
                  .ForMember(
                      dto => dto.ContentTypeAlias,
                      expression => expression.MapFrom(content => content.ContentType.Alias));

            //FROM IMember TO ContentItemDto<IMember>
            config.CreateMap<IMember, ContentItemDto<IMember>>()
                  .ForMember(
                      dto => dto.Owner,
                      expression => expression.ResolveUsing<OwnerResolver<IMember>>());
        }

        /// <summary>
        /// Maps the generic tab with custom properties for content
        /// </summary>
        /// <param name="member"></param>
        /// <param name="display"></param>
        private static void MapGenericCustomProperties(IMember member, MemberDisplay display)
        {

            TabsAndPropertiesResolver.MapGenericProperties(
                member, display,
                new ContentPropertyDisplay
                    {
                        Alias = string.Format("{0}login", Constants.PropertyEditors.InternalGenericPropertiesPrefix),
                        Label = ui.Text("login"),
                        Value = display.Username,
                        View = "textbox"
                    },
                new ContentPropertyDisplay
                    {
                        Alias = string.Format("{0}password", Constants.PropertyEditors.InternalGenericPropertiesPrefix),
                        Label = ui.Text("password"),
                        Value = "",
                        View = "changepassword" //TODO: Hard coding this because the templatepicker doesn't necessarily need to be a resolvable (real) property editor
                    },
                new ContentPropertyDisplay
                    {
                        Alias = string.Format("{0}email", Constants.PropertyEditors.InternalGenericPropertiesPrefix),
                        Label = ui.Text("general", "email"),
                        Value = display.Email,
                        View = "textbox"
                    });
        }

    }
}