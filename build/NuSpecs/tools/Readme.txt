A note about running Umbraco from Visual Studio.

When upgrading your website using nuget a backup of config files and web.config will be created and the files will
be overwritten with those from the current release. This means that you'll have to merge the files if you made any
changes to the previous config files.

It's not possible to create a deploy package from Visual Studio, which contains everything out of the box. This is
due to a number of files and folders that are not added to the Visual Studio project. These folders include:
install, umbraco and umbraco_client.
A custom deploy script will be added in a later release.

Please read the release notes on Codeplex:
http://umbraco.codeplex.com/releases

- Umbraco