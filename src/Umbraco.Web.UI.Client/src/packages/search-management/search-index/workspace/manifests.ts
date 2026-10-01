import { UMB_SEARCH_INDEX_ENTITY_TYPE, UMB_SEARCH_WORKSPACE_ALIAS } from '../constants.js';
import { manifests as viewManifests } from './views/manifests.js';

export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'workspace',
		kind: 'routable',
		alias: UMB_SEARCH_WORKSPACE_ALIAS,
		name: 'Search Workspace',
		api: () => import('./search-workspace.context.js'),
		meta: {
			entityType: UMB_SEARCH_INDEX_ENTITY_TYPE,
		},
	},
	...viewManifests,
];
