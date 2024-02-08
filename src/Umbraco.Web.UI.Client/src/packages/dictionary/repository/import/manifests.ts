import { UmbDictionaryImportRepository } from './dictionary-import.repository.js';
import type { ManifestRepository } from '@umbraco-cms/backoffice/extension-registry';

export const UMB_DICTIONARY_IMPORT_REPOSITORY_ALIAS = 'Umb.Repository.Dictionary.Import';

const repository: ManifestRepository = {
	type: 'repository',
	alias: UMB_DICTIONARY_IMPORT_REPOSITORY_ALIAS,
	name: 'Dictionary Import Repository',
	api: UmbDictionaryImportRepository,
};

export const manifests = [repository];
