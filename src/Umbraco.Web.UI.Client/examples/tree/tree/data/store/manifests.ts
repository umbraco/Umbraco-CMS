import { EXAMPLE_TREE_STORE_ALIAS } from './constants.js';

export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'treeStore',
		alias: EXAMPLE_TREE_STORE_ALIAS,
		name: 'Example Tree Store',
		api: () => import('./tree.store.js'),
	},
];
