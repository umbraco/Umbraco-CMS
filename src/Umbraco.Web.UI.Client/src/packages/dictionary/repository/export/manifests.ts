import { UMB_DICTIONARY_EXPORT_REPOSITORY_ALIAS } from './constants.js';
import type { ManifestRepository, ManifestTypes } from '@umbraco-cms/backoffice/extension-registry';

const repository: ManifestRepository = {
	type: 'repository',
	alias: UMB_DICTIONARY_EXPORT_REPOSITORY_ALIAS,
	name: 'Dictionary Export Repository',
	api: () => import('./dictionary-export.repository.js'),
};

export const manifests: Array<ManifestTypes> = [repository];
