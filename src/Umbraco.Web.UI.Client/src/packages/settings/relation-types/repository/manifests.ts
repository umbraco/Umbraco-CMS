import { UmbRelationTypeRepository } from '../repository/relation-type.repository.js';
import { UmbRelationTypeStore } from './relation-type.store.js';
import { ManifestStore, ManifestRepository } from '@umbraco-cms/backoffice/extension-registry';

export const UMB_RELATION_TYPE_REPOSITORY_ALIAS = 'Umb.Repository.RelationType';

const repository: ManifestRepository = {
	type: 'repository',
	alias: UMB_RELATION_TYPE_REPOSITORY_ALIAS,
	name: 'Relation Type Repository',
	api: UmbRelationTypeRepository,
};

export const RELATION_TYPE_STORE_ALIAS = 'Umb.Store.RelationType';

const store: ManifestStore = {
	type: 'store',
	alias: RELATION_TYPE_STORE_ALIAS,
	name: 'Relation Type Store',
	api: UmbRelationTypeStore,
};

export const manifests = [repository, store];
