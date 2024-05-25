import { UMB_LANGUAGE_COLLECTION_REPOSITORY_ALIAS } from './constants.js';
import type { ManifestRepository, ManifestTypes } from '@umbraco-cms/backoffice/extension-registry';

const repository: ManifestRepository = {
	type: 'repository',
	alias: UMB_LANGUAGE_COLLECTION_REPOSITORY_ALIAS,
	name: 'Language Collection Repository',
	api: () => import('./language-collection.repository.js'),
};

export const manifests: Array<ManifestTypes> = [repository];
