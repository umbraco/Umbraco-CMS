export const manifests: Array<UmbExtensionManifest> = [
  {
    type: 'searchIndexDetailBox',
    alias: 'Umb.SearchIndexDetailBox.Stats',
    name: 'Search Index Stats Box',
    weight: 100,
    element: '@umbraco-cms/search/settings',
    elementName: 'umb-search-index-stats-box',
    meta: {
      label: '#search_statsBoxLabel',
      column: 'right',
    },
  },
  {
    type: 'searchIndexDetailBox',
    alias: 'Umb.SearchIndexDetailBox.Search',
    name: 'Search Index Search Box',
    weight: 100,
    element: '@umbraco-cms/search/settings',
    elementName: 'umb-search-index-search-box',
    meta: {
      label: '#search_searchBoxLabel',
      column: 'left',
    },
  },
];
