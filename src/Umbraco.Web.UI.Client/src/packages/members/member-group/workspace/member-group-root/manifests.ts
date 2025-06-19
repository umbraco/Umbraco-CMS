import { UMB_MEMBER_GROUP_COLLECTION_ALIAS, UMB_MEMBER_GROUP_ROOT_ENTITY_TYPE } from '../../constants.js';
import { UMB_MEMBER_GROUP_ROOT_WORKSPACE_ALIAS } from './constants.js';
import { UMB_WORKSPACE_CONDITION_ALIAS } from '@umbraco-cms/backoffice/workspace';

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
				alias: UMB_WORKSPACE_CONDITION_ALIAS,
				match: UMB_MEMBER_GROUP_ROOT_WORKSPACE_ALIAS,
			},
		],
	},
];
