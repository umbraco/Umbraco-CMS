import {
  UMB_SEARCH_ENTITY_TYPE,
  UMB_SEARCH_ROOT_COLLECTION_ALIAS,
  UMB_SEARCH_ROOT_WORKSPACE_ALIAS,
} from '@umbraco-cms/search/global';

export const manifests: Array<UmbExtensionManifest> = [
  {
    type: 'workspace',
    kind: 'default',
    name: 'Umbraco Search - Workspace',
    alias: UMB_SEARCH_ROOT_WORKSPACE_ALIAS,
    meta: {
      entityType: UMB_SEARCH_ENTITY_TYPE,
      headline: '#search_treeHeader',
    },
  },
  {
    type: 'workspaceView',
    kind: 'collection',
    name: 'Umbraco Search - Workspace View',
    alias: 'Umbraco.Search.WorkspaceView.Collection',
    meta: {
      label: '#search_treeHeader',
      pathname: 'indexes',
      icon: 'icon-search',
      collectionAlias: UMB_SEARCH_ROOT_COLLECTION_ALIAS,
    },
    conditions: [
      {
        alias: 'Umb.Condition.WorkspaceAlias',
        match: UMB_SEARCH_ROOT_WORKSPACE_ALIAS,
      },
    ],
  },
];
