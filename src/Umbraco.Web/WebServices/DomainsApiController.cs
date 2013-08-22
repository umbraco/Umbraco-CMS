using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using Umbraco.Core;
using Umbraco.Web.WebApi;
//using umbraco.cms.businesslogic.language;
using umbraco.cms.businesslogic.web;

namespace Umbraco.Web.WebServices
{
    /// <summary>
    /// A REST controller used for managing domains.
    /// </summary>
    /// <remarks>Nothing to do with Active Directory.</remarks>
    public class DomainsApiController : UmbracoAuthorizedApiController
    {
        [HttpPost]
        // can't pass multiple complex args in json post request...
        public PostBackModel SaveLanguageAndDomains(PostBackModel model)
        {
            var node = ApplicationContext.Current.Services.ContentService.GetById(model.NodeId);

            if (node == null)
                throw new HttpResponseException(new HttpResponseMessage(HttpStatusCode.BadRequest)
                {
                    Content = new StringContent(string.Format("There is no content node with id {0}.", model.NodeId)),
                    ReasonPhrase = "Node Not Found."
                });

            if (!UmbracoUser.GetPermissions(node.Path).Contains(global::umbraco.BusinessLogic.Actions.ActionAssignDomain.Instance.Letter))
                throw new HttpResponseException(new HttpResponseMessage(HttpStatusCode.Unauthorized)
                    {
                        Content = new StringContent("You do not have permission to assign domains on that node."),
                        ReasonPhrase = "Permission Denied."
                    });

            model.Valid = true;
            var domains = Routing.DomainHelper.GetNodeDomains(model.NodeId, true);
            var languages = global::umbraco.cms.businesslogic.language.Language.GetAllAsList().ToArray();
            var language = model.Language > 0 ? languages.FirstOrDefault(l => l.id == model.Language) : null;

            // process wildcard

            if (language != null)
            {
                var wildcard = domains.FirstOrDefault(d => d.IsWildcard);
                if (wildcard != null)
                    wildcard.Language = language;
                else // yet there is a race condition here...
                    Domain.MakeNew("*" + model.NodeId, model.NodeId, model.Language);
            }
            else
            {
                var wildcard = domains.FirstOrDefault(d => d.IsWildcard);
                if (wildcard != null)
                    wildcard.Delete();
            }

            // process domains

            foreach (var domain in domains.Where(d => model.Domains.All(m => !m.Name.Equals(d.Name, StringComparison.OrdinalIgnoreCase))))
                domain.Delete();

            var names = new List<string>();

            foreach (var domainModel in model.Domains.Where(m => !string.IsNullOrWhiteSpace(m.Name)))
            {
                language = languages.FirstOrDefault(l => l.id == domainModel.Lang);
                if (language == null)
                    continue;
                var name = domainModel.Name.ToLowerInvariant();
                if (names.Contains(name))
                {
                    domainModel.Duplicate = true;
                    continue;
                }
                names.Add(name);
                var domain = domains.FirstOrDefault(d => d.Name.Equals(domainModel.Name, StringComparison.OrdinalIgnoreCase));
                if (domain != null)
                    domain.Language = language;
                else if (Domain.Exists(domainModel.Name))
                    domainModel.Duplicate = true;
                else // yet there is a race condition here...
                    Domain.MakeNew(name, model.NodeId, domainModel.Lang);
            }

            model.Valid = model.Domains.All(m => !m.Duplicate);

            return model;
        }

        #region Models

        public class PostBackModel
        {
            public bool Valid { get; set; }
            public int NodeId { get; set; }
            public int Language { get; set; }
            public DomainModel[] Domains { get; set; }
        }

        public class DomainModel
        {
            public DomainModel(string name, int lang)
            {
                Name = name;
                Lang = lang;
            }

            public string Name { get; private set; }
            public int Lang { get; private set; }
            public bool Duplicate { get; set; }
        }

        #endregion
    }
}
