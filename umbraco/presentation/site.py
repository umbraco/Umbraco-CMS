import clr
import sys
import System

clr.AddReferenceByPartialName('System.Web')

clr.AddReferenceToFile('interfaces.dll')
clr.AddReferenceToFile('cms.dll')
clr.AddReferenceToFile('umbraco.dll')

import umbraco.cms.businesslogic
import System.Web

def renderControl(controlToRender):
    sw = System.IO.StringWriter()
    writer = System.Web.UI.HtmlTextWriter(sw)
    controlToRender.Render(writer)
    return sw.ToString()