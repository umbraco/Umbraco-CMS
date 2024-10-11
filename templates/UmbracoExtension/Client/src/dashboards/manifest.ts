export const manifests: Array<UmbExtensionManifest> = [
  {
    name: "Awesome Dashboard",
    alias: "my.awesome.dashboard",
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
