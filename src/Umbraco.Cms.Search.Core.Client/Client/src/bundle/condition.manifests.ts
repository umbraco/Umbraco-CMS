export const manifests: Array<UmbExtensionManifest> = [
  {
    type: 'condition',
    name: 'Search Index Provider Name Condition',
    alias: 'Umb.Search.Condition.IndexProviderName',
    api: () =>
      import('@umbraco-cms/search/settings').then((m) => ({
        default: m.UmbSearchIndexProviderNameCondition,
      })),
  },
];
