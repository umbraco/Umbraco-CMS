using System;
using System.Data;
using System.Web;
using System.Collections;
using System.Web.Services;
using System.Web.Services.Protocols;
using System.ComponentModel;
using System.Collections.Generic;
using System.Xml.Serialization;
using System.Xml;
using umbraco.cms.businesslogic.web;
using umbraco.cms;
using umbraco.cms.businesslogic.member;

namespace umbraco.webservices.media
{

    [WebService(Namespace = "http://umbraco.org/webservices/")]
    [WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
    [ToolboxItem(false)]
    public class mediaService : BaseWebService
    {

        override public Services Service
        {
            get
            {
                return Services.MediaService;
            }
        }
        
    }
}
