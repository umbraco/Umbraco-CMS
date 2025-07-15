import { UMB_MEDIA_ENTITY_TYPE, UMB_MEDIA_ROOT_ENTITY_TYPE } from '../entity.js';
import { UMB_MEDIA_TREE_ALIAS, UMB_MEDIA_TREE_REPOSITORY_ALIAS, UMB_MEDIA_TREE_STORE_ALIAS } from './constants.js';
import { manifests as reloadTreeItemChildrenManifests } from './reload-tree-item-children/manifests.js';

export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'repository',
		alias: UMB_MEDIA_TREE_REPOSITORY_ALIAS,
		name: 'Media Tree Repository',
		api: () => import('./media-tree.repository.js'),
	},
	{
		type: 'treeStore',
		alias: UMB_MEDIA_TREE_STORE_ALIAS,
		name: 'Media Tree Store',
		api: () => import('./media-tree.store.js'),
	},
	{
		type: 'tree',
		alias: UMB_MEDIA_TREE_ALIAS,
		name: 'Media Tree',
		element: () => import('./media-tree.element.js'),
		api: () => import('./media-tree.context.js'),
		meta: {
			repositoryAlias: UMB_MEDIA_TREE_REPOSITORY_ALIAS,
		},
	},
	{
		type: 'treeItem',
		kind: 'default',
		alias: 'Umb.TreeItem.Media',
		name: 'Media Tree Item',
		element: () => import('./tree-item/media-tree-item.element.js'),
		api: () => import('./tree-item/media-tree-item.context.js'),
		forEntityTypes: [UMB_MEDIA_ENTITY_TYPE],
	},
	{
		type: 'treeItem',
		kind: 'default',
		alias: 'Umb.TreeItem.Media.Root',
		name: 'Media Tree Root',
		forEntityTypes: [UMB_MEDIA_ROOT_ENTITY_TYPE],
	},
	...reloadTreeItemChildrenManifests,
];
