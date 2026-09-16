import {
  UMB_SEARCH_COLLECTION_REPOSITORY_ALIAS,
  UMB_SEARCH_DETAIL_REPOSITORY_ALIAS,
  UMB_SEARCH_DETAIL_STORE_ALIAS,
  UMB_SEARCH_QUERY_REPOSITORY_ALIAS,
} from '@umbraco-cms/search/global';

export const manifests: Array<UmbExtensionManifest> = [
  {
    type: 'repository',
    name: 'Umbraco Search Collection Repository',
    alias: UMB_SEARCH_COLLECTION_REPOSITORY_ALIAS,
    api: () =>
      import('@umbraco-cms/search/settings').then((m) => ({
        default: m.UmbSearchCollectionRepository,
      })),
  },
  {
    type: 'repository',
    name: 'Umbraco Search Detail Repository',
    alias: UMB_SEARCH_DETAIL_REPOSITORY_ALIAS,
    api: () =>
      import('@umbraco-cms/search/settings').then((m) => ({
        default: m.UmbSearchDetailRepository,
      })),
  },
  {
    type: 'repository',
    name: 'Umbraco Search Query Repository',
    alias: UMB_SEARCH_QUERY_REPOSITORY_ALIAS,
    api: () =>
      import('@umbraco-cms/search/settings').then((m) => ({ default: m.UmbSearchQueryRepository })),
  },
  {
    type: 'store',
    name: 'Umbraco Search Detail Store',
    alias: UMB_SEARCH_DETAIL_STORE_ALIAS,
    api: () =>
      import('@umbraco-cms/search/settings').then((m) => ({ default: m.UmbSearchDetailStore })),
  },
];
