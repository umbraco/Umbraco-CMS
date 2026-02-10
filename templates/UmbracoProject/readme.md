# Central Package Management

This project supports [Central Package Management (CPM)](https://learn.microsoft.com/en-us/nuget/consume-packages/central-package-management) as an optional feature when creating from the template.

## Without CPM (default)
When you create a project without CPM (default, or -P Traditional):
- Package versions are specified directly in the .csproj file
- No Directory.Packages.props file is created
- This is the traditional NuGet package management approach

## With CPM enabled
When you create a project with CPM enabled (-P Central):

- Package versions are defined in `Directory.Packages.props`
- The `.csproj` file references packages without specifying versions
- This ensures consistent versions across all projects in your solution

### Important Note

`Directory.Packages.props` is discovered by directory traversal (from project folder upward).

If you move projects into subfolders, ensure `Directory.Packages.props` remains in a parent directory of all projects, or copy it to the appropriate location.

If you add an Umbraco extension project to this solution, you may need to:
1. Move `Directory.Packages.props` to the solution root folder
2. Merge package versions from both projects into one file

### Example structure
```
MySolution/
├── Directory.Packages.props  ← Place here for multiple projects
├── MyWeb/
│   └── MyWeb.csproj
└── MyLibrary/
    └── MyLibrary.csproj
```

### Updating package versions

Edit `Directory.Packages.props` to change versions:
```xml
<PackageVersion Include="Umbraco.Cms" Version="x.x.x" />
```

### Learn more

- [Central Package Management - Microsoft Docs](https://learn.microsoft.com/en-us/nuget/consume-packages/central-package-management)
