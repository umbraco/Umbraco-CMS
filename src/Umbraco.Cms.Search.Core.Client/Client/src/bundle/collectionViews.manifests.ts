import {
  UMB_SEARCH_COLLECTION_REPOSITORY_ALIAS,
  UMB_SEARCH_COLLECTION_VIEW_ALIAS,
  UMB_SEARCH_ROOT_COLLECTION_ALIAS,
} from '@umbraco-cms/search/global';

export const manifests: Array<UmbExtensionManifest> = [
  {
    type: 'collection',
    kind: 'default',
    name: 'Umbraco Search - Root Collection',
    alias: UMB_SEARCH_ROOT_COLLECTION_ALIAS,
    api: () =>
      import('@umbraco-cms/search/settings').then((m) => ({
        default: m.UmbSearchCollectionContext,
      })),
    meta: {
      repositoryAlias: UMB_SEARCH_COLLECTION_REPOSITORY_ALIAS,
    },
  },
  {
    type: 'collectionView',
    name: 'Umbraco Search - Root Collection View',
    alias: UMB_SEARCH_COLLECTION_VIEW_ALIAS,
    element: '@umbraco-cms/search/settings',
    elementName: 'umb-search-root-collection-view',
    meta: {
      label: '#search_treeHeader',
      icon: 'icon-search',
      pathName: 'table',
    },
    conditions: [
      {
        alias: 'Umb.Condition.CollectionAlias',
        match: UMB_SEARCH_ROOT_COLLECTION_ALIAS,
      },
    ],
  },
];
