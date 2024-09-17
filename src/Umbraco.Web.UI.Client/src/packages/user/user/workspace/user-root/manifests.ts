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
		alias: 'Umb.WorkspaceView.UserRoot.Collection',
		name: 'User Root Collection Workspace View',
		element: () => import('./collection-workspace-view.element.js'),
		meta: {
			label: 'Collection',
			pathname: 'collection',
			icon: 'icon-layers',
		},
		conditions: [
			{
				alias: 'Umb.Condition.WorkspaceAlias',
				match: UMB_USER_ROOT_WORKSPACE_ALIAS,
			},
		],
	},
];
