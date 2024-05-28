import { UMB_DICTIONARY_IMPORT_REPOSITORY_ALIAS } from './constants.js';
import { UmbDictionaryImportRepository } from './dictionary-import.repository.js';
import type { ManifestRepository, ManifestTypes } from '@umbraco-cms/backoffice/extension-registry';

const repository: ManifestRepository = {
	type: 'repository',
	alias: UMB_DICTIONARY_IMPORT_REPOSITORY_ALIAS,
	name: 'Dictionary Import Repository',
	api: UmbDictionaryImportRepository,
};

export const manifests: Array<ManifestTypes> = [repository];
