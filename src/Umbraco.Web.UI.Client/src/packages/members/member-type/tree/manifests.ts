import { UMB_MEMBER_TYPE_ENTITY_TYPE, UMB_MEMBER_TYPE_ROOT_ENTITY_TYPE } from '../entity.js';
import { UMB_MEMBER_TYPE_ROOT_WORKSPACE_ALIAS } from '../member-type-root/constants.js';
import {
	UMB_MEMBER_TYPE_TREE_ALIAS,
	UMB_MEMBER_TYPE_TREE_REPOSITORY_ALIAS,
	UMB_MEMBER_TYPE_TREE_STORE_ALIAS,
} from './constants.js';
import { UmbMemberTypeTreeStore } from './member-type-tree.store.js';
import { manifests as folderManifests } from './folder/manifests.js';
import { manifests as treeItemChildrenManifests } from './tree-item-children/manifests.js';
import { manifests as viewManifests } from './views/manifests.js';
import { UMB_MEMBER_TYPE_FOLDER_WORKSPACE_ALIAS } from './folder/workspace/constants.js';
import { UMB_WORKSPACE_CONDITION_ALIAS } from '@umbraco-cms/backoffice/workspace';
import { UMB_TREE_ALIAS_CONDITION } from '@umbraco-cms/backoffice/tree';
import type { UmbExtensionManifestKind } from '@umbraco-cms/backoffice/extension-registry';

export const manifests: Array<UmbExtensionManifest | UmbExtensionManifestKind> = [
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
		api: UmbMemberTypeTreeStore,
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
		type: 'treeAction',
		kind: 'create',
		name: 'Member Type Tree Create Action',
		alias: 'Umb.TreeAction.MemberType.Create',
		conditions: [
			{
				alias: UMB_TREE_ALIAS_CONDITION,
				match: UMB_MEMBER_TYPE_TREE_ALIAS,
			},
		],
	},
	{
		type: 'workspaceView',
		kind: 'tree',
		alias: 'Umb.WorkspaceView.MemberType.Tree',
		name: 'Member Type Tree Item Children Workspace View',
		meta: {
			label: '#tree_children',
			pathname: 'children',
			icon: 'icon-bulleted-list',
			treeAlias: UMB_MEMBER_TYPE_TREE_ALIAS,
		},
		conditions: [
			{
				alias: UMB_WORKSPACE_CONDITION_ALIAS,
				oneOf: [UMB_MEMBER_TYPE_ROOT_WORKSPACE_ALIAS, UMB_MEMBER_TYPE_FOLDER_WORKSPACE_ALIAS],
			},
		],
	},
	...folderManifests,
	...treeItemChildrenManifests,
	...viewManifests,
];
