using System;
using System.Collections.Generic;
using System.Web.Http;
using System.Web.Http.Controllers;
using System.Web.Http.ModelBinding;
using AutoMapper;
using Newtonsoft.Json;
using Umbraco.Core.Logging;
using Umbraco.Core.Services;
using Umbraco.Web.Models.ContentEditing;
using Umbraco.Web.Mvc;
using Umbraco.Web.WebApi;
using System.Linq;
using Umbraco.Core.Models.EntityBase;
using Umbraco.Core.Models;
using Umbraco.Web.WebApi.Filters;

using Constants = Umbraco.Core.Constants;

using System.Xml;
using Umbraco.Core.IO;
using Umbraco.Core.Models.Membership;

namespace Umbraco.Web.Editors
{
    /// <summary>
    /// The API controller used for getting entity objects, basic name, icon, id representation of any umbraco object
    /// </summary>
    [PluginController("UmbracoApi")]
    public class DashboardController : UmbracoAuthorizedJsonController
    {

        public IEnumerable<Tab<Control>> GetDashboard(string section)
        {
            return GetDashboardFromXml(section);

            //return Mapper.Map<EntityBasic>(Services.EntityService.Get(id, UmbracoObjectTypes.Document));
        }

        //TODO migrate this into a more managed class
        private IEnumerable<Tab<Control>> GetDashboardFromXml(string section)
        {

            XmlDocument dashBoardXml = new XmlDocument();
            dashBoardXml.Load(IOHelper.MapPath(SystemFiles.DashboardConfig));
            var user = UmbracoContext.Security.CurrentUser;
            var tabs = new List<Tab<Control>>();

            // test for new tab interface
            foreach (XmlNode dashboard in dashBoardXml.DocumentElement.SelectNodes("//section [areas/area = '" + section.ToLower() + "']"))
            {
                if (dashboard != null)
                {
                    var i = 0;

                    foreach (XmlNode entry in dashboard.SelectNodes("./tab"))
                    {

                        if (ValidateAccess(entry, user))
                        {
                            i++;

                            Tab<Control> tab = new Tab<Control>();
                            tab.Label = entry.Attributes.GetNamedItem("caption").Value;
                            var props = new List<Control>();
                            tab.Id = i;
                            tab.Alias = tab.Label.ToLower().Replace(" ", "_");
                            tab.IsActive = i == 1;

                            foreach (XmlNode uc in entry.SelectNodes("./control"))
                            {
                                if (ValidateAccess(uc, user))
                                {
                                    Control ctrl = new Control();
                                    
                                    string control = Core.XmlHelper.GetNodeValue(uc).Trim(' ', '\r', '\n');
                                    ctrl.Path = IOHelper.FindFile(control);
                                    if (control.ToLower().EndsWith(".ascx"))
                                        ctrl.ServerSide = true;

                                    props.Add(ctrl);
                                    
                                    /*
                                    try
                                    {
                                        Control c = LoadControl(path);

                                        // set properties
                                        Type type = c.GetType();
                                        if (type != null)
                                        {
                                            foreach (XmlAttribute att in uc.Attributes)
                                            {
                                                string attributeName = att.Name;
                                                string attributeValue = parseControlValues(att.Value).ToString();
                                                // parse special type of values


                                                PropertyInfo prop = type.GetProperty(attributeName);
                                                if (prop == null)
                                                {
                                                    continue;
                                                }

                                                prop.SetValue(c, Convert.ChangeType(attributeValue, prop.PropertyType),
                                                              null);

                                            }
                                        }

                                        //resolving files from dashboard config which probably does not map to a virtual fi
                                        tab.Controls.Add(AddPanel(uc, c));
                                    }
                                    catch (Exception ee)
                                    {
                                        tab.Controls.Add(
                                            new LiteralControl(
                                                "<p class=\"umbracoErrorMessage\">Could not load control: '" + path +
                                                "'. <br/><span class=\"guiDialogTiny\"><strong>Error message:</strong> " +
                                                ee.ToString() + "</span></p>"));
                                    }*/
                                }
                            }
                            tab.Properties = props;
                            tabs.Add(tab);
                        }


                    }
                }
                
            }

            return tabs;

        }


        //TODO: This has to go away, jesus
        //for now I'm just returning true, this is likely to change anyway
        private bool ValidateAccess(XmlNode node, IUser currentUser)
        {
            return true;
    
            /*            
            // check if this area should be shown at all
            string onlyOnceValue = StateHelper.GetCookieValue(generateCookieKey(node));
            if (!String.IsNullOrEmpty(onlyOnceValue))
            {
                return false;
            }

            // the root user can always see everything
            if (currentUser.IsRoot())
            {
                return true;
            }
            else if (node != null)
            {
                XmlNode accessRules = node.SelectSingleNode("access");
                bool retVal = true;
                if (accessRules != null && accessRules.HasChildNodes)
                {
                    string currentUserType = CurrentUser.UserType.Alias.ToLowerInvariant();

                    //Update access rules so we'll be comparing lower case to lower case always

                    var denies = accessRules.SelectNodes("deny");
                    foreach (XmlNode deny in denies)
                    {
                        deny.InnerText = deny.InnerText.ToLowerInvariant();
                    }

                    var grants = accessRules.SelectNodes("grant");
                    foreach (XmlNode grant in grants)
                    {
                        grant.InnerText = grant.InnerText.ToLowerInvariant();
                    }

                    string allowedSections = ",";
                    foreach (BusinessLogic.Application app in CurrentUser.Applications)
                    {
                        allowedSections += app.alias.ToLower() + ",";
                    }
                    XmlNodeList grantedTypes = accessRules.SelectNodes("grant");
                    XmlNodeList grantedBySectionTypes = accessRules.SelectNodes("grantBySection");
                    XmlNodeList deniedTypes = accessRules.SelectNodes("deny");

                    // if there's a grant type, everyone who's not granted is automatically denied
                    if (grantedTypes.Count > 0 || grantedBySectionTypes.Count > 0)
                    {
                        retVal = false;
                        if (grantedBySectionTypes.Count > 0 && accessRules.SelectSingleNode(String.Format("grantBySection [contains('{0}', concat(',',.,','))]", allowedSections)) != null)
                        {
                            retVal = true;
                        }
                        else if (grantedTypes.Count > 0 && accessRules.SelectSingleNode(String.Format("grant [. = '{0}']", currentUserType)) != null)
                        {
                            retVal = true;
                        }
                    }
                    // if the current type of user is denied we'll say nay
                    if (deniedTypes.Count > 0 && accessRules.SelectSingleNode(String.Format("deny [. = '{0}']", currentUserType)) != null)
                    {
                        retVal = false;
                    }

                }

                return retVal;
            }
            return false;
             * */
        }

    }
}
