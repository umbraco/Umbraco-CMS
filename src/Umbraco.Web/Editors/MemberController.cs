using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Examine.LuceneEngine.SearchCriteria;
using Examine.SearchCriteria;

using umbraco.cms.businesslogic.member;
using Umbraco.Web.Models.ContentEditing;
using Umbraco.Web.Mvc;
using Umbraco.Web.WebApi.Filters;
using Constants = Umbraco.Core.Constants;
using Examine;
using System.Web.Security;

namespace Umbraco.Web.Editors
{
    /// <remarks>
    /// This controller is decorated with the UmbracoApplicationAuthorizeAttribute which means that any user requesting
    /// access to ALL of the methods on this controller will need access to the member application.
    /// </remarks>
    [PluginController("UmbracoApi")]
    [UmbracoApplicationAuthorizeAttribute(Constants.Applications.Members)]
    public class MemberController : UmbracoAuthorizedJsonController
    {
        public IEnumerable<MemberEntityBasic> Search(string query)
        {
            if (string.IsNullOrEmpty(query))
                throw new ArgumentException("A search query must be defined", "query");


            //this will change when we add the new members API, but for now, we need to live with this setup
            if (Member.InUmbracoMemberMode())
            {
                var internalSearcher = ExamineManager.Instance.SearchProviderCollection[Constants.Examine.InternalMemberSearcher];
                var criteria = internalSearcher.CreateSearchCriteria("member", BooleanOperation.Or);
                var fields = new[] { "id", "__nodeName", "email" };
                var term = new[] { query.ToLower().Escape() };
                var operation = criteria.GroupedOr(fields, term).Compile();

                var results = internalSearcher.Search(operation)
                    .Select(x => new MemberEntityBasic
                    {
                        Id = int.Parse(x["id"]),
                        Name = x["nodeName"],
                        Email = x["email"],
                        LoginName = x["loginName"],
                        Icon = ".icon-user"
                    });

                    return results;
            }
            else
            {
                IEnumerable<MemberEntityBasic> results;
                
                if (query.Contains("@"))
                {
                    results = from MembershipUser x in Membership.FindUsersByEmail(query)
                              select
                                  new MemberEntityBasic()
                                  {
                                      //how do we get ID? 
                                      Id = 0,
                                      Email = x.Email,
                                      LoginName = x.UserName,
                                      Name = x.UserName,
                                      Icon = "icon-user"
                                  };
                }
                else
                {
                    results = from MembershipUser x in Membership.FindUsersByName(query + "%")
                              select
                                  new MemberEntityBasic()
                                  {
                                      //how do we get ID? 
                                      Id = 0,
                                      Email = x.Email,
                                      LoginName = x.UserName,
                                      Name = x.UserName,
                                      Icon = "icon-user"
                                  };
                }

                return results;
            }
        }

    }
}
