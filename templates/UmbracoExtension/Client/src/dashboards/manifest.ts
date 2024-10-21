export const manifests: Array<UmbExtensionManifest> = [
  {
    name: "UmbracoExtension Dashboard",
    alias: "PROJECT_NAME_KEBABCASE_FOR_NPM.dashboard",
    type: 'dashboard',
    js: () => import("./dashboard.element"),
    meta: {
      label: "Awesome Dashboard",
      pathname: "my-awesome-dashboard"
    },
    conditions: [
      {
        alias: 'Umb.Condition.SectionAlias',
        match: 'Umb.Section.Content',
      }
    ],
  }
];
