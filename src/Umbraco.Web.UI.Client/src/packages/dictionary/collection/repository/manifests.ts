import { UmbDictionaryCollectionRepository } from './dictionary-collection.repository.js';
import type { ManifestRepository } from '@umbraco-cms/backoffice/extension-registry';

export const UMB_DICTIONARY_COLLECTION_REPOSITORY_ALIAS = 'Umb.Repository.Dictionary.Collection';

const repository: ManifestRepository = {
	type: 'repository',
	alias: UMB_DICTIONARY_COLLECTION_REPOSITORY_ALIAS,
	name: 'Dictionary Collection Repository',
	api: UmbDictionaryCollectionRepository,
};

export const manifests = [repository];
