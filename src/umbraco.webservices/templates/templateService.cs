using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Web.Services;
using Umbraco.Core;
using Umbraco.Web;
using Umbraco.Web.Cache;

namespace umbraco.webservices.templates
{
    /// <summary>
    /// Summary description for TemplateService
    /// </summary>
    [WebService(Namespace = "http://umbraco.org/webservices/")]
    [WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
    [ToolboxItem(false)]
    public class templateService : BaseWebService
    {

        override public Services Service
        {
            get
            {
                return Services.TemplateService;
            }
        }

        [WebMethod]
        public int readIdFromAlias(string alias, string username, string password)
        {
            Authenticate(username, password);

            cms.businesslogic.template.Template template;

            try
            {
                template = cms.businesslogic.template.Template.GetByAlias(alias, true);
            }
            catch (Exception)
            {
                throw new Exception("Could not load template from alias: " + alias);
            }

            if (template == null)
            {
                throw new Exception("Could not load template from alias: " + alias);
            }

            return template.Id;
        }


        [WebMethod]
        public List<templateCarrier> readList(string username, string password)
        {
            Authenticate(username, password);

            return cms.businesslogic.template.Template.GetAllAsList().Select(createTemplateCarrier).ToList();
        }



        [WebMethod]
        public void delete(int id, string username, string password)
        {
            Authenticate(username, password);

            if (id == 0)
                throw new Exception("ID must be specifed when updating");

            var template = new cms.businesslogic.template.Template(id);

            template.delete();
        }

        [WebMethod]
        public int create(templateCarrier carrier, string username, string password)
        {
            Authenticate(username, password);

            if (carrier.Id != 0) throw new Exception("ID may not be specified when creating");
            if (carrier == null) throw new Exception("No carrier specified");

            // Get the user
            BusinessLogic.User user = GetUser(username, password);

            // Create template
            var template = cms.businesslogic.template.Template.MakeNew(carrier.Name, user);

            template.MasterTemplate = carrier.MastertemplateId;
            template.Alias = carrier.Alias;
            template.Text = carrier.Name;
            template.Design = carrier.Design;
            template.Save();
            return template.Id;
        }

        [WebMethod]
        public void update(templateCarrier carrier, string username, string password)
        {
            Authenticate(username, password);

            if (carrier.Id == 0) throw new Exception("ID must be specifed when updating");
            if (carrier == null) throw new Exception("No carrier specified");

            cms.businesslogic.template.Template template;

            try
            {
                template = new cms.businesslogic.template.Template(carrier.Id);
            }
            catch (Exception)
            {
                throw new Exception("Template with ID " + carrier.Id + " not found");
            }

            template.MasterTemplate = carrier.MastertemplateId;
            template.Alias = carrier.Alias;
            template.Text = carrier.Name;
            template.Design = carrier.Design;
            template.Save();
        }

        [WebMethod]
        public templateCarrier read(int id, string username, string password)
        {
            Authenticate(username, password);

            cms.businesslogic.template.Template template;

            try
            {
                template = new cms.businesslogic.template.Template(id);
            }
            catch (Exception)
            {
                throw new Exception("Template with ID " + id + " not found");
            }

            if (template == null)
                throw new Exception("Template with ID " + id + " not found");

            return createTemplateCarrier(template);
        }

        public templateCarrier createTemplateCarrier(cms.businesslogic.template.Template template)
        {
            var carrier = new templateCarrier
                                {
                                    Id = template.Id,
                                    MastertemplateId = template.MasterTemplate,
                                    Alias = template.Alias,
                                    Name = template.Text,
                                    Design = template.Design,
                                    MasterPageFile = template.MasterPageFile
                                };
            return carrier;
        }


        public class templateCarrier
        {
            public int Id { get; set; }
            public int MastertemplateId { get; set; }
            public string MasterPageFile { get; set; }
            public string Name { get; set; }
            public string Alias { get; set; }
            public string Design { get; set; }
        }
    }
}