      _       _              _                                                          
     | |     | |            | |                                                         
   __| | ___ | |_ _ __   ___| |_   _ __   _____      __                                 
  / _` |/ _ \| __| '_ \ / _ \ __| | '_ \ / _ \ \ /\ / /                                 
 | (_| | (_) | |_| | | |  __/ |_  | | | |  __/\ V  V /                                  
  \__,_|\___/ \__|_| |_|\___|\__| |_| |_|\___| \_/\_/   _                 _             
                 | |                                   | |               (_)            
  _   _ _ __ ___ | |__  _ __ __ _  ___ ___     _____  _| |_ ___ _ __  ___ _  ___  _ __  
 | | | | '_ ` _ \| '_ \| '__/ _` |/ __/ _ \   / _ \ \/ / __/ _ \ '_ \/ __| |/ _ \| '_ \ 
 | |_| | | | | | | |_) | | | (_| | (_| (_) | |  __/>  <| ||  __/ | | \__ \ | (_) | | | |
  \__,_|_| |_| |_|_.__/|_|  \__,_|\___\___/   \___/_/\_\\__\___|_| |_|___/_|\___/|_| |_|
                                                                                        

== Requirements ==
* Node LTS Version 20.17.0+

== Steps ==
* Open `\Client` folder in Visual Studio Code
* Open a terminal inside the `\Client` folder
* Run `npm install` to install all the dependencies
* Run `npm run build` to build the project
* The build output is copied to `wwwroot\App_Plugins\UmbracoExtension\PROJECT_NAME_KEBABCASE_FOR_NPM.js`

== File Watching ==
* Add this Razor Class Library Project as a project reference to an Umbraco Website project
* From the `\Client` folder run the command `npm run watch` this will monitor the changes to the *.ts files and rebuild the project
* With the Umbraco website project running the Razor Class Library Project will refresh the browser when the build is complete

== Other Resources ==
* Umbraco Docs - https://docs.umbraco.com/umbraco-cms/customizing/extend-and-customize-editing-experience
