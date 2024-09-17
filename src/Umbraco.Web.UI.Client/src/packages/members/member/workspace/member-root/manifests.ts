import { UMB_MEMBER_ROOT_ENTITY_TYPE } from '../../entity.js';
import { UMB_MEMBER_ROOT_WORKSPACE_ALIAS } from './constants.js';

export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'workspace',
		kind: 'default',
		alias: UMB_MEMBER_ROOT_WORKSPACE_ALIAS,
		name: 'Member Root Workspace',
		meta: {
			entityType: UMB_MEMBER_ROOT_ENTITY_TYPE,
			headline: '#treeHeaders_member',
		},
	},
	{
		type: 'workspaceView',
		alias: 'Umb.WorkspaceView.MemberRoot.Collection',
		name: 'Member Root Collection Workspace View',
		element: () => import('./member-root-workspace.element.js'),
		meta: {
			label: 'Collection',
			pathname: 'collection',
			icon: 'icon-layers',
		},
		conditions: [
			{
				alias: 'Umb.Condition.WorkspaceAlias',
				match: UMB_MEMBER_ROOT_WORKSPACE_ALIAS,
			},
		],
	},
];
