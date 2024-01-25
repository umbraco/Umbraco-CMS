import {
	UMB_DOCUMENT_TYPE_ENTITY_TYPE,
	UMB_DOCUMENT_TYPE_FOLDER_ENTITY_TYPE,
	UMB_DOCUMENT_TYPE_ROOT_ENTITY_TYPE,
} from '../entity.js';
import { UmbDocumentTypeTreeRepository } from './document-type-tree.repository.js';
import { UmbDocumentTypeTreeStore } from './document-type.tree.store.js';
import { manifests as folderManifests } from './folder/manifests.js';
import { manifests as reloadManifests } from './reload-tree-item-children/manifests.js';
import type {
	ManifestRepository,
	ManifestTree,
	ManifestTreeItem,
	ManifestTreeStore,
} from '@umbraco-cms/backoffice/extension-registry';

export const UMB_DOCUMENT_TYPE_TREE_REPOSITORY_ALIAS = 'Umb.Repository.DocumentType.Tree';
export const UMB_DOCUMENT_TYPE_TREE_STORE_ALIAS = 'Umb.Store.DocumentType.Tree';
export const UMB_DOCUMENT_TYPE_TREE_ALIAS = 'Umb.Tree.DocumentType';

const treeRepository: ManifestRepository = {
	type: 'repository',
	alias: UMB_DOCUMENT_TYPE_TREE_REPOSITORY_ALIAS,
	name: 'Document Type Tree Repository',
	api: UmbDocumentTypeTreeRepository,
};

const treeStore: ManifestTreeStore = {
	type: 'treeStore',
	alias: UMB_DOCUMENT_TYPE_TREE_STORE_ALIAS,
	name: 'Document Type Tree Store',
	api: UmbDocumentTypeTreeStore,
};

const tree: ManifestTree = {
	type: 'tree',
	alias: UMB_DOCUMENT_TYPE_TREE_ALIAS,
	name: 'Document Type Tree',
	meta: {
		repositoryAlias: UMB_DOCUMENT_TYPE_TREE_REPOSITORY_ALIAS,
	},
};

const treeItem: ManifestTreeItem = {
	type: 'treeItem',
	kind: 'unique',
	alias: 'Umb.TreeItem.DocumentType',
	name: 'Document Type Tree Item',
	meta: {
		entityTypes: [
			UMB_DOCUMENT_TYPE_ROOT_ENTITY_TYPE,
			UMB_DOCUMENT_TYPE_ENTITY_TYPE,
			UMB_DOCUMENT_TYPE_FOLDER_ENTITY_TYPE,
		],
	},
};

export const manifests = [treeRepository, treeStore, tree, treeItem, ...folderManifests, ...reloadManifests];
