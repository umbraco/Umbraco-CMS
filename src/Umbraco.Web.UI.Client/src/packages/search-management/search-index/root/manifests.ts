import {
	UMB_SEARCH_ROOT_COLLECTION_ALIAS,
	UMB_SEARCH_ROOT_ENTITY_TYPE,
	UMB_SEARCH_ROOT_WORKSPACE_ALIAS,
} from '../constants.js';
import { UMB_ADVANCED_SETTINGS_MENU_ALIAS } from '@umbraco-cms/backoffice/settings';

export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'workspace',
		kind: 'default',
		name: 'Umbraco Search - Workspace',
		alias: UMB_SEARCH_ROOT_WORKSPACE_ALIAS,
		meta: {
			entityType: UMB_SEARCH_ROOT_ENTITY_TYPE,
			headline: '#searchManagement_treeHeader',
		},
	},
	{
		type: 'workspaceView',
		kind: 'collection',
		name: 'Umbraco Search - Workspace View',
		// Kept on the spelling it shipped with in the standalone Umbraco.Cms.Search package.
		// Unlike the repository and store aliases, an extension that renders cannot be registered
		// under a second alias without appearing twice, so this one cannot be renamed and shimmed.
		// eslint-disable-next-line local-rules/enforce-manifest-alias
		alias: 'Umbraco.Search.WorkspaceView.Collection',
		meta: {
			label: '#searchManagement_treeHeader',
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
	{
		type: 'menuItem',
		name: 'Umbraco Search Root Menu Item',
		// Kept on the spelling it shipped with in the standalone Umbraco.Cms.Search package.
		// Unlike the repository and store aliases, an extension that renders cannot be registered
		// under a second alias without appearing twice, so this one cannot be renamed and shimmed.
		// eslint-disable-next-line local-rules/enforce-manifest-alias
		alias: 'Umbraco.Search.Root.MenuItem',
		meta: {
			label: '#searchManagement_treeHeader',
			entityType: UMB_SEARCH_ROOT_ENTITY_TYPE,
			icon: 'icon-search',
			menus: [UMB_ADVANCED_SETTINGS_MENU_ALIAS],
		},
	},
];
