A note about running Umbraco from Visual Studio.

When upgrading your website using nuget a backup of config files and web.config will be created. Only the web.config will
be overwritten by default to ensure that it has the necessary settings from the current release. This means that you'll 
have to merge the files if you made any changes to the previous config files.
The config files found in the config folder will usually not be changed for patch releases, so they can usually be skipped,
but the web.config will have to have its previous "umbracoConfigurationStatus"-appsetting and "umbracoDbDSN" connection string
copied over (as a minimum).

It's not possible to create a deploy package from Visual Studio, which contains everything out of the box. This is
due to a number of files and folders that are not added to the Visual Studio project. These folders include:
install, umbraco and umbraco_client.
A custom deploy script will be added in a later release.

Please read the release notes on our.umbraco.org:
http://our.umbraco.org/contribute/releases

- Umbraco