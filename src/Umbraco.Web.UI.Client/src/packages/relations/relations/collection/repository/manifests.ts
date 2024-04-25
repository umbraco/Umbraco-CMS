import type { ManifestRepository, ManifestTypes } from '@umbraco-cms/backoffice/extension-registry';

export const UMB_RELATION_COLLECTION_REPOSITORY_ALIAS = 'Umb.Repository.Relation.Collection';

const repository: ManifestRepository = {
	type: 'repository',
	alias: UMB_RELATION_COLLECTION_REPOSITORY_ALIAS,
	name: 'Relation Collection Repository',
	api: () => import('./relation-collection.repository.js'),
};

export const manifests: Array<ManifestTypes> = [repository];
