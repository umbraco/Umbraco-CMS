
  _    _ __  __ ____  _____           _____ ____  
 | |  | |  \/  |  _ \|  __ \    /\   / ____/ __ \ 
 | |  | | \  / | |_) | |__) |  /  \ | |   | |  | |
 | |  | | |\/| |  _ <|  _  /  / /\ \| |   | |  | |
 | |__| | |  | | |_) | | \ \ / ____ | |___| |__| |
  \____/|_|  |_|____/|_|  \_/_/    \_\_____\____/ 
                                                   
----------------------------------------------------

Umbraco extensibility code has been installed for ASP.Net Identity with Umbraco back office users

The files have been installed into your App_Start folder if you have a Web Application project 
or into App_Code if you have a Website project. 

All of these files include lots of code comments, documentation & notes to assist with extending
the ASP.Net Identity implementaion for back office users in Umbraco. For all 3rd party 
ASP.Net providers, their dependencies will need to be manually installed. See comments in the
following files for full details:

* StandardUmbracoOwinStartup.cs 	Includes code snippets to enable 3rd party ASP.Net Identity 
									providers to work with the Umbraco back office.
									To enable the 'StandardUmbracoOwinStartup', update the web.config
									appSetting "owin:appStartup" to be: "StandardUmbracoOwinStartup"

* UmbracoCustomOwinStartup			Includes code snippets to customize the Umbraco ASP.Net 
									Identity implementation for back office users as well as 
									snippets to enable 3rd party ASP.Net Identity providers to work.
									To enable the 'UmbracoCustomOwinStartup', update the web.config
									appSetting "owin:appStartup" to be: "UmbracoCustomOwinStartup"

* UmbracoBackOfficeAuthExtensions	Includes extension methods snippets to enable 3rd party ASP.Net
									Identity providers to work with the Umbraco back office. 