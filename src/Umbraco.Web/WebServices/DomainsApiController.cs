using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using Umbraco.Core;
using Umbraco.Web.WebApi;
//using umbraco.cms.businesslogic.language;
using umbraco.BusinessLogic.Actions;
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
            {
                var response = Request.CreateResponse(HttpStatusCode.BadRequest);
                response.Content = new StringContent(string.Format("There is no content node with id {0}.", model.NodeId));
                response.ReasonPhrase = "Node Not Found.";
                throw new HttpResponseException(response);
            }

            if (UmbracoUser.GetPermissions(node.Path).Contains(ActionAssignDomain.Instance.Letter) == false)
            {
                var response = Request.CreateResponse(HttpStatusCode.BadRequest);
                response.Content = new StringContent("You do not have permission to assign domains on that node.");
                response.ReasonPhrase = "Permission Denied.";
                throw new HttpResponseException(response);
            }

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

            // delete every (non-wildcard) domain, that exists in the DB yet is not in the model
            foreach (var domain in domains.Where(d => d.IsWildcard == false && model.Domains.All(m => m.Name.Equals(d.Name, StringComparison.OrdinalIgnoreCase) == false)))
                domain.Delete();

            var names = new List<string>();

            // create or update domains in the model
            foreach (var domainModel in model.Domains.Where(m => string.IsNullOrWhiteSpace(m.Name) == false))
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

            model.Valid = model.Domains.All(m => m.Duplicate == false);

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
