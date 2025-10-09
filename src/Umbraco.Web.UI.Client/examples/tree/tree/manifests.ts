import { EXAMPLE_ENTITY_TYPE, EXAMPLE_ROOT_ENTITY_TYPE } from '../entity.js';
import { EXAMPLE_TREE_ALIAS } from './constants.js';
import { EXAMPLE_TREE_REPOSITORY_ALIAS } from './data/constants.js';
import { manifests as dataManifests } from './data/manifests.js';

export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'tree',
		kind: 'default',
		alias: EXAMPLE_TREE_ALIAS,
		name: 'Example Tree',
		meta: {
			repositoryAlias: EXAMPLE_TREE_REPOSITORY_ALIAS,
		},
	},
	{
		type: 'treeItem',
		kind: 'default',
		alias: 'Example.TreeItem',
		name: 'Example Tree Item',
		forEntityTypes: [EXAMPLE_ROOT_ENTITY_TYPE, EXAMPLE_ENTITY_TYPE],
	},
	...dataManifests,
];
