import { UMB_ELEMENT_ENTITY_TYPE } from '../../entity.js';
import { UMB_ELEMENT_RECYCLE_BIN_ROOT_ENTITY_TYPE } from '../root/constants.js';
import { UMB_ELEMENT_RECYCLE_BIN_TREE_ALIAS, UMB_ELEMENT_RECYCLE_BIN_TREE_REPOSITORY_ALIAS } from './constants.js';
import { manifests as dataManifests } from './data/manifests.js';

export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'tree',
		kind: 'default',
		alias: UMB_ELEMENT_RECYCLE_BIN_TREE_ALIAS,
		name: 'Element Recycle Bin Tree',
		meta: {
			repositoryAlias: UMB_ELEMENT_RECYCLE_BIN_TREE_REPOSITORY_ALIAS,
		},
	},
	{
		type: 'treeItem',
		kind: 'recycleBin',
		alias: 'Umb.TreeItem.Element.RecycleBin',
		name: 'Element Recycle Bin Tree Item',
		forEntityTypes: [UMB_ELEMENT_RECYCLE_BIN_ROOT_ENTITY_TYPE],
		meta: {
			supportedEntityTypes: [UMB_ELEMENT_ENTITY_TYPE],
		},
	},
	...dataManifests,
];
