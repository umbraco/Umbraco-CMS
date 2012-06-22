using System;
using System.Data;
using System.Web;
using System.Collections;
using System.Web.Services;
using System.Web.Services.Protocols;
using System.ComponentModel;
using System.Collections.Generic;
using umbraco.cms.businesslogic.web;
using umbraco.cms;
using umbraco;


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
        public int readIdFromAlias(string alias ,string username, string password)
        {
            Authenticate(username, password);
           
            cms.businesslogic.template.Template template ;

            try
            {
               template = cms.businesslogic.template.Template.GetByAlias(alias);
            }
            catch ( Exception  )
            {
                 throw new Exception( "Could not load template from alias: " + alias);
            }
            if (template == null)
            {
                 throw new Exception( "Could not load template from alias: " + alias);
            }

            return template.Id;
        }


        [WebMethod]
        public List<templateCarrier> readList(string username, string password)
        {
            List<templateCarrier> templateCarriers = new List<templateCarrier>();

            Authenticate(username, password);

            foreach (cms.businesslogic.template.Template template in cms.businesslogic.template.Template.getAll())
            {
                templateCarriers.Add(createTemplateCarrier(template));
            }
            return templateCarriers;
        }



        [WebMethod]
        public void delete(int id, string username, string password)
        {
            Authenticate(username, password);

            if (id == 0) throw new Exception("ID must be specifed when updating");

            cms.businesslogic.template.Template template = new cms.businesslogic.template.Template(id);
            template.delete();
        }

        [WebMethod]
        public int create(templateCarrier carrier, string username, string password)
        {
            Authenticate(username, password);
            
            if (carrier.Id != 0) throw new Exception("ID may not be specified when creating");
            if (carrier == null) throw new Exception("No carrier specified");

            // Get the user
            umbraco.BusinessLogic.User user = GetUser(username, password);

            // Create template
            cms.businesslogic.template.Template template = cms.businesslogic.template.Template.MakeNew(carrier.Name, user);
               
            template.MasterTemplate = carrier.MastertemplateId;
            template.Alias = carrier.Alias;
            template.Text = carrier.Name;
            template.Design = carrier.Design;
            template.Save();
            clearCachedTemplate(template);
            return template.Id;
        }

        [WebMethod]
        public void update(templateCarrier carrier, string username, string password)
        {
            
          
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
            template.Text = carrier.Name ;
            template.Design = carrier.Design;
            template.Save();
            clearCachedTemplate(template);
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
            {
                throw new Exception("Template with ID " + id + " not found");
            }

            return createTemplateCarrier(template);
        }

        public templateCarrier createTemplateCarrier(cms.businesslogic.template.Template template)
        {
            templateCarrier carrier = new templateCarrier();
            carrier.Id = template.Id;
            carrier.MastertemplateId = template.MasterTemplate;
            carrier.Alias = template.Alias;
            carrier.Name = template.Text;
            carrier.Design = template.Design;
            return carrier;
        }


        public class templateCarrier
        {
            private int id;
            private int mastertemplateId;
            private string name;
            private string alias;
            private string design;

            public int Id
            {
                get { return id; }
                set { id = value; }
            }

            public int MastertemplateId
            {
                get { return mastertemplateId; }
                set { mastertemplateId = value; }
            }

            public string Name
            {
                get { return name; }
                set { name = value; }
            }

            public string Alias
            {
                get { return alias; }
                set { alias = value; }
            }

            public string Design
            {
                get { return design; }
                set { design = value; }
            }
        }

        private void clearCachedTemplate(cms.businesslogic.template.Template cachedTemplate)
        {
            // Clear cache in rutime
            if (UmbracoSettings.UseDistributedCalls)
                umbraco.presentation.cache.dispatcher.Refresh(
                    new Guid("dd12b6a0-14b9-46e8-8800-c154f74047c8"),
                    cachedTemplate.Id);
            else
                umbraco.template.ClearCachedTemplate(cachedTemplate.Id);
        }

    }
}
