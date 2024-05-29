import { UMB_DICTIONARY_COLLECTION_REPOSITORY_ALIAS } from './constants.js';
import type { ManifestRepository, ManifestTypes } from '@umbraco-cms/backoffice/extension-registry';

const repository: ManifestRepository = {
	type: 'repository',
	alias: UMB_DICTIONARY_COLLECTION_REPOSITORY_ALIAS,
	name: 'Dictionary Collection Repository',
	api: () => import('./dictionary-collection.repository.js'),
};

export const manifests: Array<ManifestTypes> = [repository];
