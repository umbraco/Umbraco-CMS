import { EXAMPLE_TREE_REPOSITORY_ALIAS, EXAMPLE_TREE_STORE_ALIAS } from './constants.js';

export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'repository',
		alias: EXAMPLE_TREE_REPOSITORY_ALIAS,
		name: 'Example Tree Repository',
		api: () => import('./tree.repository.js'),
	},
	{
		type: 'treeStore',
		alias: EXAMPLE_TREE_STORE_ALIAS,
		name: 'Example Tree Store',
		api: () => import('./tree.store.js'),
	},
];
