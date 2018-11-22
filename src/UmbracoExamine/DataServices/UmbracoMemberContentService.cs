using System;
using System.Collections.Generic;
using System.Linq;
using Umbraco.Core;
using Umbraco.Core.Logging;

namespace UmbracoExamine.DataServices
{
    public class UmbracoMemberContentService : UmbracoContentService
    {
        public override IEnumerable<string> GetAllUserPropertyNames()
        {
            using (var scope = ApplicationContext.Current.ScopeProvider.CreateScope())
            {
                try
                {
                    //only return the property type aliases for members

                    var result = ApplicationContext.DatabaseContext.Database.Fetch<string>(
                        @"select distinct cmsPropertyType.alias from cmsPropertyType 
                        inner join cmsContentType on cmsContentType.nodeId = cmsPropertyType.contentTypeId
                        inner join umbracoNode on umbracoNode.id = cmsContentType.nodeId
                        where umbracoNode.nodeObjectType = @nodeObjectType
                        order by alias", new {nodeObjectType = Constants.ObjectTypes.MemberType});

                    scope.Complete();
                    return result;
                }
                catch (Exception ex)
                {
                    LogHelper.Error<UmbracoMemberContentService>("EXCEPTION OCCURRED reading GetAllUserPropertyNames", ex);
                    return Enumerable.Empty<string>();
                }
            }
        }
    }
}