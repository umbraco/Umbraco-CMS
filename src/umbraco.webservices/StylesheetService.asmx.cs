using System;
using System.Data;
using System.Web;
using System.Collections;
using System.Web.Services;
using System.Web.Services.Protocols;
using System.ComponentModel;
using umbraco.cms.businesslogic.web;
using umbraco.cms;
using umbraco;
using System.Collections.Generic;

namespace umbraco.webservices.stylesheets
{
    /// <summary>
    /// Summary description for StylesheetService
    /// </summary>
    [WebService(Namespace = "http://umbraco.org/webservices/")]
    [WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
    [ToolboxItem(false)]
    public class stylesheetService : BaseWebService
    {

        override public Services Service
        {
            get
            {
                return Services.StylesheetService;
            }
        }

        
        [WebMethod]
        public int create(stylesheetCarrier carrier, string username, string password)
        {
            Authenticate(username, password);
            umbraco.BusinessLogic.User user = GetUser(username, password);

            StyleSheet stylesheet = StyleSheet.MakeNew(user, carrier.Name, (carrier.Name + ".css"), carrier.Content );
            stylesheet.saveCssToFile();

            return stylesheet.Id;
        }


        [WebMethod]
        public stylesheetCarrier read(int id, string username, string password)
        {
            Authenticate(username, password);

            StyleSheet stylesheet;
            try
            {
                stylesheet = new StyleSheet(id);
            }
            catch (Exception)
            {
                throw new Exception("Could not load stylesheet with id: " + id + ", not found");
            }

            if (stylesheet == null)
                throw new Exception("Could not load stylesheet with id: " + id + ", not found");

            return createCarrier(stylesheet);
        }


        [WebMethod]
        public List<stylesheetCarrier> readList(string username, string password)
        {
            Authenticate(username, password);
            
            List<stylesheetCarrier> stylesheets = new List<stylesheetCarrier>();
                        
            StyleSheet[] foundstylesheets = StyleSheet.GetAll();

            foreach (StyleSheet stylesheet in foundstylesheets)
            {
                stylesheets.Add(createCarrier(stylesheet));
            }

            return stylesheets;
        }


        [WebMethod]
        public void update(stylesheetCarrier carrier, string username, string password)
        {
            Authenticate(username, password);

            StyleSheet stylesheet = null;
            try
            {
                stylesheet = new StyleSheet(carrier.Id);
            }
            catch
            {}

            if (stylesheet == null)
                throw new Exception("Could not load stylesheet with id: " + carrier.Id );

            stylesheet.Content = carrier.Content;
            stylesheet.saveCssToFile();
        }


        private stylesheetCarrier createCarrier(StyleSheet stylesheet)
        {
            stylesheetCarrier carrier = new stylesheetCarrier();
            carrier.Id = stylesheet.Id;
            carrier.Name = stylesheet.Text;
            carrier.Content = stylesheet.Content;
            return carrier;
        }



        public class stylesheetCarrier
        {
            private int id;
            private string name;
            private string content;

            public int Id
            {
                get { return id; }
                set { id = value; }
            }

            public string Name
            {
                get { return name; }
                set { name = value; }
            }

            public string Content
            {
                get { return content; }
                set { content = value; }
            }
        }
    }
}
