import { EXAMPLE_TREE_REPOSITORY_ALIAS } from './constants.js';

export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'repository',
		alias: EXAMPLE_TREE_REPOSITORY_ALIAS,
		name: 'Example Tree Repository',
		api: () => import('./tree.repository.js'),
	},
];
