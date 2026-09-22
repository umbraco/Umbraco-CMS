import {
	UMB_MEDIA_TYPE_ENTITY_TYPE,
	UMB_MEDIA_TYPE_ROOT_ENTITY_TYPE,
	UMB_MEDIA_TYPE_FOLDER_ENTITY_TYPE,
} from '../entity.js';
import { UMB_MEDIA_TYPE_ROOT_WORKSPACE_ALIAS } from '../media-type-root/constants.js';
import {
	UMB_MEDIA_TYPE_TREE_ALIAS,
	UMB_MEDIA_TYPE_TREE_REPOSITORY_ALIAS,
	UMB_MEDIA_TYPE_TREE_STORE_ALIAS,
} from './constants.js';
import { UmbMediaTypeTreeStore } from './media-type-tree.store.js';
import { manifests as folderManifests } from './folder/manifests.js';
import { manifests as treeItemChildrenManifest } from './tree-item-children/manifests.js';
import { manifests as viewManifests } from './views/manifests.js';
import { UMB_MEDIA_TYPE_FOLDER_WORKSPACE_ALIAS } from './folder/workspace/constants.js';
import { UMB_WORKSPACE_CONDITION_ALIAS } from '@umbraco-cms/backoffice/workspace';
import { UMB_TREE_ALIAS_CONDITION } from '@umbraco-cms/backoffice/tree';
import type { UmbExtensionManifestKind } from '@umbraco-cms/backoffice/extension-registry';

export const manifests: Array<UmbExtensionManifest | UmbExtensionManifestKind> = [
	{
		type: 'repository',
		alias: UMB_MEDIA_TYPE_TREE_REPOSITORY_ALIAS,
		name: 'Media Type Tree Repository',
		api: () => import('./media-type-tree.repository.js'),
	},
	{
		type: 'treeStore',
		alias: UMB_MEDIA_TYPE_TREE_STORE_ALIAS,
		name: 'Media Type Tree Store',
		api: UmbMediaTypeTreeStore,
	},
	{
		type: 'tree',
		kind: 'default',
		alias: UMB_MEDIA_TYPE_TREE_ALIAS,
		name: 'Media Type Tree',
		meta: {
			repositoryAlias: UMB_MEDIA_TYPE_TREE_REPOSITORY_ALIAS,
		},
	},
	{
		type: 'treeItem',
		kind: 'default',
		alias: 'Umb.TreeItem.MediaType',
		name: 'Media Type Tree Item',
		forEntityTypes: [UMB_MEDIA_TYPE_ENTITY_TYPE, UMB_MEDIA_TYPE_ROOT_ENTITY_TYPE, UMB_MEDIA_TYPE_FOLDER_ENTITY_TYPE],
	},
	{
		type: 'treeAction',
		kind: 'create',
		name: 'Media Type Tree Create Action',
		alias: 'Umb.TreeAction.MediaType.Create',
		conditions: [
			{
				alias: UMB_TREE_ALIAS_CONDITION,
				match: UMB_MEDIA_TYPE_TREE_ALIAS,
			},
		],
	},
	{
		type: 'workspaceView',
		kind: 'tree',
		alias: 'Umb.WorkspaceView.MediaType.Tree',
		name: 'Media Type Tree Item Children Workspace View',
		meta: {
			label: '#tree_children',
			pathname: 'children',
			icon: 'icon-bulleted-list',
			treeAlias: UMB_MEDIA_TYPE_TREE_ALIAS,
		},
		conditions: [
			{
				alias: UMB_WORKSPACE_CONDITION_ALIAS,
				oneOf: [UMB_MEDIA_TYPE_ROOT_WORKSPACE_ALIAS, UMB_MEDIA_TYPE_FOLDER_WORKSPACE_ALIAS],
			},
		],
	},
	...folderManifests,
	...treeItemChildrenManifest,
	...viewManifests,
];
