import { UMB_MEMBER_GROUP_ROOT_ENTITY_TYPE } from '../../entity.js';
import { UMB_MEMBER_GROUP_ROOT_WORKSPACE_ALIAS } from './constants.js';

export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'workspace',
		kind: 'default',
		alias: UMB_MEMBER_GROUP_ROOT_WORKSPACE_ALIAS,
		name: 'Member Group Root Workspace View',
		meta: {
			entityType: UMB_MEMBER_GROUP_ROOT_ENTITY_TYPE,
			headline: '#treeHeaders_memberGroups',
		},
	},
	{
		type: 'workspaceView',
		alias: 'Umb.WorkspaceView.MemberGroupRoot.Collection',
		name: 'Member Group Root Collection Workspace View',
		element: () => import('./member-group-root-workspace.element.js'),
		meta: {
			label: 'Collection',
			pathname: 'collection',
			icon: 'icon-layers',
		},
		conditions: [
			{
				alias: 'Umb.Condition.WorkspaceAlias',
				match: UMB_MEMBER_GROUP_ROOT_WORKSPACE_ALIAS,
			},
		],
	},
];
