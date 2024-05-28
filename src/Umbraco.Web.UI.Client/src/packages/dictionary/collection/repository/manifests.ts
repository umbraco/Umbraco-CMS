import type { ManifestRepository, ManifestTypes } from '@umbraco-cms/backoffice/extension-registry';

export const UMB_DICTIONARY_COLLECTION_REPOSITORY_ALIAS = 'Umb.Repository.Dictionary.Collection';

const repository: ManifestRepository = {
	type: 'repository',
	alias: UMB_DICTIONARY_COLLECTION_REPOSITORY_ALIAS,
	name: 'Dictionary Collection Repository',
	api: () => import('./dictionary-collection.repository.js'),
};

export const manifests: Array<ManifestTypes> = [repository];
