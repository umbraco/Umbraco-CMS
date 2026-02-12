import {
	UMB_STATIC_FILE_ENTITY_TYPE,
	UMB_STATIC_FILE_FOLDER_ENTITY_TYPE,
	UMB_STATIC_FILE_ROOT_ENTITY_TYPE,
	UMB_STATIC_FILE_TREE_ALIAS,
	UMB_STATIC_FILE_TREE_ITEM_ALIAS,
	UMB_STATIC_FILE_TREE_REPOSITORY_ALIAS,
} from './constants.js';

export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'repository',
		alias: UMB_STATIC_FILE_TREE_REPOSITORY_ALIAS,
		name: 'Static File Tree Repository',
		api: () => import('./static-file-tree.repository.js'),
	},
	{
		type: 'tree',
		kind: 'default',
		alias: UMB_STATIC_FILE_TREE_ALIAS,
		name: 'Static File Tree',
		meta: {
			repositoryAlias: UMB_STATIC_FILE_TREE_REPOSITORY_ALIAS,
		},
	},
	{
		type: 'treeItem',
		kind: 'default',
		alias: UMB_STATIC_FILE_TREE_ITEM_ALIAS,
		name: 'Static File Tree Item',
		forEntityTypes: [UMB_STATIC_FILE_ENTITY_TYPE, UMB_STATIC_FILE_ROOT_ENTITY_TYPE, UMB_STATIC_FILE_FOLDER_ENTITY_TYPE],
	},
];
