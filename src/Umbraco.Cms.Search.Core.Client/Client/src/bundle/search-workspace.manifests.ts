import {
  UMB_SEARCH_WORKSPACE_ALIAS,
  UMB_SEARCH_INDEX_ENTITY_TYPE,
} from '@umbraco-cms/search/global';
import { UMB_WORKSPACE_CONDITION_ALIAS } from '@umbraco-cms/backoffice/workspace';

export const manifests: Array<UmbExtensionManifest> = [
  {
    type: 'workspace',
    kind: 'routable',
    alias: UMB_SEARCH_WORKSPACE_ALIAS,
    name: 'Search Workspace',
    api: () =>
      import('@umbraco-cms/search/settings').then((m) => ({
        default: m.UmbSearchWorkspaceContext,
      })),
    meta: {
      entityType: UMB_SEARCH_INDEX_ENTITY_TYPE,
    },
  },
  {
    type: 'workspaceView',
    alias: 'Umb.WorkspaceView.Search.Details',
    name: 'Search Details View',
    element: '@umbraco-cms/search/settings',
    elementName: 'umb-search-details-view',
    weight: 300,
    meta: {
      label: '#general_details',
      pathname: 'details',
      icon: 'icon-search',
    },
    conditions: [
      {
        alias: UMB_WORKSPACE_CONDITION_ALIAS,
        match: UMB_SEARCH_WORKSPACE_ALIAS,
      },
    ],
  },
];
