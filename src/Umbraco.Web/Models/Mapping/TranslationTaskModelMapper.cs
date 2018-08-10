using AutoMapper;
using Umbraco.Core;
using Umbraco.Core.Models;
using Umbraco.Core.Models.Mapping;
using Umbraco.Core.Models.Membership;
using Umbraco.Core.Services;
using Umbraco.Web.Models.ContentEditing;

namespace Umbraco.Web.Models.Mapping
{
    /// <summary>
    /// 
    /// </summary>
    internal class TranslationTaskModelMapper : MapperConfiguration
    {
        public override void ConfigureMappings(IConfiguration config, ApplicationContext applicationContext)
        {
            config.CreateMap<Task, TaskDisplay>()
                .ForMember(task => task.CreatedDate, expression => expression.MapFrom(x => x.CreateDate))
                .AfterMap((source, dest) =>
                {
                    var document = applicationContext.Services.ContentService.GetById(source.EntityId);
                    var assignedBy = applicationContext.Services.UserService.GetUserById(source.OwnerUserId);
                    var assignedTo = applicationContext.Services.UserService.GetUserById(source.AssigneeUserId);

                    dest.AssignedBy = Mapper.Map<IUser, UserDisplay>(assignedBy);
                    dest.AssignedTo = Mapper.Map<IUser, UserDisplay>(assignedTo);
                    dest.TotalWords = applicationContext.Services.ContentService.CountWords(document);
                    dest.EntityId = document.Id;

                    dest.Properties.Add(new PropertyDisplay { Name = applicationContext.Services.TextService.Localize("nodeName"), Value = document.Name });

                    foreach (var prop in document.Properties)
                    {
                        if (prop.Value is string asString)
                        {
                            dest.Properties.Add(new PropertyDisplay { Name = prop.Alias, Value = asString });
                        }
                    }
                });
        }
    }
}
