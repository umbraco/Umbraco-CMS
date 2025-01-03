import { UMB_USER_GROUP_COLLECTION_ALIAS } from '../../collection/index.js';
import { UMB_USER_GROUP_ROOT_ENTITY_TYPE } from '../../entity.js';
import { UMB_USER_GROUP_ROOT_WORKSPACE_ALIAS } from './constants.js';
import { UMB_WORKSPACE_CONDITION_ALIAS } from '@umbraco-cms/backoffice/workspace';

export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'workspace',
		kind: 'default',
		alias: UMB_USER_GROUP_ROOT_WORKSPACE_ALIAS,
		name: 'User Group Root Workspace View',
		meta: {
			entityType: UMB_USER_GROUP_ROOT_ENTITY_TYPE,
			headline: '#user_usergroups',
		},
	},
	{
		type: 'workspaceView',
		kind: 'collection',
		alias: 'Umb.WorkspaceView.UserGroupRoot.Collection',
		name: 'User Group Root Collection Workspace View',
		meta: {
			label: 'Collection',
			pathname: 'collection',
			icon: 'icon-layers',
			collectionAlias: UMB_USER_GROUP_COLLECTION_ALIAS,
		},
		conditions: [
			{
				alias: UMB_WORKSPACE_CONDITION_ALIAS,
				match: UMB_USER_GROUP_ROOT_WORKSPACE_ALIAS,
			},
		],
	},
];
