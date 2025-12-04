# Contributing to the Umbraco Templates

If you're making changes to any of the templates in this folder, please note that installing them directly from this location to test will not work. You need to 'pack' the source into a nuget package (.nupkg) in a local folder, and install from there.

For example, using a folder location of `c:\nuget.local`:

```
# Pack the templates to a local folder
dotnet pack -o "c:\nuget.local"

# Add this folder as a local nuget source 
dotnet nuget add source "c:\nuget.local" --name "local nuget"

# Make sure you don't have the global templates installed
dotnet new uninstall Umbraco.Templates

# Install your version of the templates, having checked/updated the name of the generated .nupkg file
dotnet new install "c:\nuget.local\Umbraco.Templates.XXXX.nupkg"
```

You can now test your template changes using the appropriate `dotnet new` command.