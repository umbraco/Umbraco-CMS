import { UmbDictionaryExportRepository } from './dictionary-export.repository.js';
import type { ManifestRepository } from '@umbraco-cms/backoffice/extension-registry';

export const UMB_DICTIONARY_EXPORT_REPOSITORY_ALIAS = 'Umb.Repository.Dictionary.Export';

const repository: ManifestRepository = {
	type: 'repository',
	alias: UMB_DICTIONARY_EXPORT_REPOSITORY_ALIAS,
	name: 'Dictionary Export Repository',
	api: UmbDictionaryExportRepository,
};

export const manifests = [repository];
