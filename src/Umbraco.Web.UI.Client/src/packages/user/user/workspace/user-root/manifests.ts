import { UMB_USER_COLLECTION_ALIAS } from '../../collection/constants.js';
import { UMB_USER_ROOT_ENTITY_TYPE } from '../../entity.js';
import { UMB_USER_ROOT_WORKSPACE_ALIAS } from './constants.js';

export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'workspace',
		kind: 'default',
		alias: UMB_USER_ROOT_WORKSPACE_ALIAS,
		name: 'User Root Workspace',
		meta: {
			entityType: UMB_USER_ROOT_ENTITY_TYPE,
			headline: '#treeHeaders_users',
		},
	},
	{
		type: 'workspaceView',
		kind: 'collection',
		alias: 'Umb.WorkspaceView.UserRoot.Collection',
		name: 'User Root Collection Workspace View',
		meta: {
			label: 'Collection',
			icon: 'icon-layers',
			pathname: 'collection',
			collectionAlias: UMB_USER_COLLECTION_ALIAS,
		},
		conditions: [
			{
				alias: 'Umb.Condition.WorkspaceAlias',
				match: UMB_USER_ROOT_WORKSPACE_ALIAS,
			},
		],
	},
];
