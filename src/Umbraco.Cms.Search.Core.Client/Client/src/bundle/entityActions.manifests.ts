import { UMB_SEARCH_INDEX_ENTITY_TYPE } from '@umbraco-cms/search/global';

export const manifests: Array<UmbExtensionManifest> = [
  {
    type: 'entityAction',
    kind: 'default',
    alias: 'Umb.Search.EntityAction.RebuildIndex',
    name: 'Umbraco Search Entity Action - Rebuild Index',
    weight: 100,
    api: () =>
      import('@umbraco-cms/search/settings').then((m) => ({
        default: m.UmbSearchRebuildIndexEntityAction,
      })),
    forEntityTypes: [UMB_SEARCH_INDEX_ENTITY_TYPE],
    meta: {
      icon: 'icon-refresh',
      label: '#search_entityActionRebuildIndex',
      additionalOptions: false,
    },
  },
];
