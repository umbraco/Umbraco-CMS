import { UmbRelationTypeRepository } from '../repository/relation-type.repository.js';
import { UmbRelationTypeStore } from './relation-type.store.js';
import { UmbRelationTypeTreeStore } from './relation-type.tree.store.js';
import { ManifestStore, ManifestTreeStore, ManifestRepository } from '@umbraco-cms/backoffice/extension-registry';

export const RELATION_TYPE_REPOSITORY_ALIAS = 'Umb.Repository.RelationType';

const repository: ManifestRepository = {
	type: 'repository',
	alias: RELATION_TYPE_REPOSITORY_ALIAS,
	name: 'Relation Type Repository',
	class: UmbRelationTypeRepository,
};

export const RELATION_TYPE_STORE_ALIAS = 'Umb.Store.RelationType';
export const RELATION_TYPE_TREE_STORE_ALIAS = 'Umb.Store.RelationTypeTree';

const store: ManifestStore = {
	type: 'store',
	alias: RELATION_TYPE_STORE_ALIAS,
	name: 'Relation Type Store',
	class: UmbRelationTypeStore,
};

const treeStore: ManifestTreeStore = {
	type: 'treeStore',
	alias: RELATION_TYPE_TREE_STORE_ALIAS,
	name: 'Relation Type Tree Store',
	class: UmbRelationTypeTreeStore,
};

export const manifests = [repository, store, treeStore];
