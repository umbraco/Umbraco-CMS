import { UMB_DICTIONARY_EXPORT_REPOSITORY_ALIAS } from './constants.js';
import { UmbDictionaryExportRepository } from './dictionary-export.repository.js';
import type { ManifestRepository, ManifestTypes } from '@umbraco-cms/backoffice/extension-registry';

const repository: ManifestRepository = {
	type: 'repository',
	alias: UMB_DICTIONARY_EXPORT_REPOSITORY_ALIAS,
	name: 'Dictionary Export Repository',
	api: UmbDictionaryExportRepository,
};

export const manifests: Array<ManifestTypes> = [repository];
