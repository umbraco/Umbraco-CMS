export const manifests: Array<UmbExtensionManifest> = [
  {
    name: "Umbraco ExtensionDashboard",
    alias: "Umbraco.Extension.Dashboard",
    type: 'dashboard',
    js: () => import("./dashboard.element"),
    meta: {
      label: "UmbracoExtension Dashboard",
      pathname: "UmbracoExtension-dashboard"
    },
    conditions: [
      {
        alias: 'Umb.Condition.SectionAlias',
        match: 'Umb.Section.Content',
      }
    ],
  }
];
