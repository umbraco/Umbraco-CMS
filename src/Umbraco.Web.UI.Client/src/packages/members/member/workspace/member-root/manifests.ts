import { UMB_MEMBER_COLLECTION_ALIAS } from '../../collection/manifests.js';
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
		kind: 'collection',
		alias: 'Umb.WorkspaceView.MemberRoot.Collection',
		name: 'Member Root Collection Workspace View',
		meta: {
			label: 'Collection',
			pathname: 'collection',
			icon: 'icon-layers',
			collectionAlias: UMB_MEMBER_COLLECTION_ALIAS,
		},
		conditions: [
			{
				alias: 'Umb.Condition.WorkspaceAlias',
				match: UMB_MEMBER_ROOT_WORKSPACE_ALIAS,
			},
		],
	},
];
