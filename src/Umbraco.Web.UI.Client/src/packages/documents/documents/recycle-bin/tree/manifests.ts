import { UMB_DOCUMENT_RECYCLE_BIN_TREE_ALIAS, UMB_DOCUMENT_RECYCLE_BIN_TREE_REPOSITORY_ALIAS } from './constants.js';
import { manifests as dataManifests } from './data/manifests.js';
import { manifests as reloadTreeItemChildrenManifests } from './reload-tree-item-children/manifests.js';
import { manifests as rootTreeItemManifests } from './tree-item/manifests.js';
import { manifests as treeItemChildrenManifests } from './tree-item-children/manifests.js';

export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'tree',
		kind: 'default',
		alias: UMB_DOCUMENT_RECYCLE_BIN_TREE_ALIAS,
		name: 'Document Recycle Bin Tree',
		meta: {
			repositoryAlias: UMB_DOCUMENT_RECYCLE_BIN_TREE_REPOSITORY_ALIAS,
		},
	},
	...dataManifests,
	...reloadTreeItemChildrenManifests,
	...rootTreeItemManifests,
	...treeItemChildrenManifests,
];
