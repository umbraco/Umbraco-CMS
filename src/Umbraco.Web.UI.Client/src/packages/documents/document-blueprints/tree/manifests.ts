import {
	UMB_DOCUMENT_BLUEPRINT_ENTITY_TYPE,
	UMB_DOCUMENT_BLUEPRINT_FOLDER_ENTITY_TYPE,
	UMB_DOCUMENT_BLUEPRINT_ROOT_ENTITY_TYPE,
} from '../entity.js';
import {
	UMB_DOCUMENT_BLUEPRINT_TREE_ALIAS,
	UMB_DOCUMENT_BLUEPRINT_TREE_REPOSITORY_ALIAS,
	UMB_DOCUMENT_BLUEPRINT_TREE_STORE_ALIAS,
} from './constants.js';
import { manifests as folderManifests } from './folder/manifests.js';
import { manifests as reloadManifests } from './reload-tree-item-children/manifests.js';
import type {
	ManifestRepository,
	ManifestTree,
	ManifestTreeItem,
	ManifestTreeStore,
	ManifestTypes,
} from '@umbraco-cms/backoffice/extension-registry';

const treeRepository: ManifestRepository = {
	type: 'repository',
	alias: UMB_DOCUMENT_BLUEPRINT_TREE_REPOSITORY_ALIAS,
	name: 'Document Blueprint Tree Repository',
	api: () => import('./document-blueprint-tree.repository.js'),
};

const treeStore: ManifestTreeStore = {
	type: 'treeStore',
	alias: UMB_DOCUMENT_BLUEPRINT_TREE_STORE_ALIAS,
	name: 'Document Blueprint Tree Store',
	api: () => import('./document-blueprint-tree.store.js'),
};

const tree: ManifestTree = {
	type: 'tree',
	kind: 'default',
	alias: UMB_DOCUMENT_BLUEPRINT_TREE_ALIAS,
	name: 'Document Blueprints Tree',
	meta: {
		repositoryAlias: UMB_DOCUMENT_BLUEPRINT_TREE_REPOSITORY_ALIAS,
	},
};

const treeItem: ManifestTreeItem = {
	type: 'treeItem',
	kind: 'default',
	alias: 'Umb.TreeItem.DocumentBlueprint',
	name: 'Document Blueprint Tree Item',
	forEntityTypes: [
		UMB_DOCUMENT_BLUEPRINT_ROOT_ENTITY_TYPE,
		UMB_DOCUMENT_BLUEPRINT_ENTITY_TYPE,
		UMB_DOCUMENT_BLUEPRINT_FOLDER_ENTITY_TYPE,
	],
};

export const manifests: Array<ManifestTypes> = [
	treeRepository,
	treeStore,
	tree,
	treeItem,
	...reloadManifests,
	...folderManifests,
];
