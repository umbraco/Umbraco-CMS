import { UMB_ELEMENT_TREE_ALIAS, UMB_ELEMENT_TREE_REPOSITORY_ALIAS } from './constants.js';
import { manifests as folderManifests } from './folder/manifests.js';

export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'repository',
		alias: UMB_ELEMENT_TREE_REPOSITORY_ALIAS,
		name: 'Element Tree Repository',
		api: () => import('./element-tree.repository.js'),
	},
	{
		type: 'tree',
		kind: 'default',
		alias: UMB_ELEMENT_TREE_ALIAS,
		name: 'Element Tree',
		meta: {
			repositoryAlias: UMB_ELEMENT_TREE_REPOSITORY_ALIAS,
		},
	},
	...folderManifests,
];
