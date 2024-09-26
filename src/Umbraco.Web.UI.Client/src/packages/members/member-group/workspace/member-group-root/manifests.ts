import { UMB_MEMBER_GROUP_COLLECTION_ALIAS } from '../../collection/manifests.js';
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
		kind: 'collection',
		alias: 'Umb.WorkspaceView.MemberGroupRoot.Collection',
		name: 'Member Group Root Collection Workspace View',
		meta: {
			label: 'Collection',
			pathname: 'collection',
			icon: 'icon-layers',
			collectionAlias: UMB_MEMBER_GROUP_COLLECTION_ALIAS,
		},
		conditions: [
			{
				alias: 'Umb.Condition.WorkspaceAlias',
				match: UMB_MEMBER_GROUP_ROOT_WORKSPACE_ALIAS,
			},
		],
	},
];
