import { UMB_MEMBER_TYPE_ROOT_WORKSPACE_ALIAS } from '../constants.js';
import { UMB_MEMBER_TYPE_ENTITY_TYPE, UMB_MEMBER_TYPE_ROOT_ENTITY_TYPE } from '../entity.js';
import {
	UMB_MEMBER_TYPE_TREE_ALIAS,
	UMB_MEMBER_TYPE_TREE_ITEM_CHILDREN_COLLECTION_ALIAS,
	UMB_MEMBER_TYPE_TREE_REPOSITORY_ALIAS,
	UMB_MEMBER_TYPE_TREE_STORE_ALIAS,
} from './constants.js';
import { manifests as treeItemChildrenManifest } from './tree-item-children/manifests.js';

export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'repository',
		alias: UMB_MEMBER_TYPE_TREE_REPOSITORY_ALIAS,
		name: 'Member Type Tree Repository',
		api: () => import('./member-type-tree.repository.js'),
	},
	{
		type: 'treeStore',
		alias: UMB_MEMBER_TYPE_TREE_STORE_ALIAS,
		name: 'Member Type Tree Store',
		api: () => import('./member-type-tree.store.js'),
	},
	{
		type: 'tree',
		kind: 'default',
		alias: UMB_MEMBER_TYPE_TREE_ALIAS,
		name: 'Member Type Tree',
		meta: {
			repositoryAlias: UMB_MEMBER_TYPE_TREE_REPOSITORY_ALIAS,
		},
	},
	{
		type: 'treeItem',
		kind: 'default',
		alias: 'Umb.TreeItem.MemberType',
		name: 'Member Type Tree Item',
		forEntityTypes: [UMB_MEMBER_TYPE_ROOT_ENTITY_TYPE, UMB_MEMBER_TYPE_ENTITY_TYPE],
	},
	{
		type: 'workspaceView',
		kind: 'collection',
		alias: 'Umb.WorkspaceView.MemberType.TreeItemChildrenCollection',
		name: 'Member Type Tree Item Children Collection Workspace View',
		meta: {
			label: '#general_design',
			pathname: 'design',
			icon: 'icon-member-dashed-line',
			collectionAlias: UMB_MEMBER_TYPE_TREE_ITEM_CHILDREN_COLLECTION_ALIAS,
		},
		conditions: [
			{
				alias: 'Umb.Condition.WorkspaceAlias',
				oneOf: [UMB_MEMBER_TYPE_ROOT_WORKSPACE_ALIAS],
			},
		],
	},
	...treeItemChildrenManifest,
];
