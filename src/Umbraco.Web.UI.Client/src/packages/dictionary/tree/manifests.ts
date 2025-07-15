import { UMB_DICTIONARY_ENTITY_TYPE, UMB_DICTIONARY_ROOT_ENTITY_TYPE } from '../entity.js';
import {
	UMB_DICTIONARY_TREE_ALIAS,
	UMB_DICTIONARY_TREE_REPOSITORY_ALIAS,
	UMB_DICTIONARY_TREE_STORE_ALIAS,
} from './constants.js';
import { manifests as reloadTreeItemChildrenManifests } from './reload-tree-item-children/manifests.js';

export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'repository',
		alias: UMB_DICTIONARY_TREE_REPOSITORY_ALIAS,
		name: 'Dictionary Tree Repository',
		api: () => import('./dictionary-tree.repository.js'),
	},
	{
		type: 'treeStore',
		alias: UMB_DICTIONARY_TREE_STORE_ALIAS,
		name: 'Dictionary Tree Store',
		api: () => import('./dictionary-tree.store.js'),
	},
	{
		type: 'tree',
		kind: 'default',
		alias: UMB_DICTIONARY_TREE_ALIAS,
		name: 'Dictionary Tree',
		meta: {
			repositoryAlias: UMB_DICTIONARY_TREE_REPOSITORY_ALIAS,
		},
	},
	{
		type: 'treeItem',
		kind: 'default',
		alias: 'Umb.TreeItem.Dictionary',
		name: 'Dictionary Tree Item',
		forEntityTypes: [UMB_DICTIONARY_ROOT_ENTITY_TYPE, UMB_DICTIONARY_ENTITY_TYPE],
	},
	...reloadTreeItemChildrenManifests,
];
