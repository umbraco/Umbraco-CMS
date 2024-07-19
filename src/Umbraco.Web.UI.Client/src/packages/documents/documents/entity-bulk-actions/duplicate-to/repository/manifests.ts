import { UMB_BULK_DUPLICATE_DOCUMENT_REPOSITORY_ALIAS } from './constants.js';
import type { ManifestRepository, ManifestTypes } from '@umbraco-cms/backoffice/extension-registry';

const bulkDuplicateRepository: ManifestRepository = {
	type: 'repository',
	alias: UMB_BULK_DUPLICATE_DOCUMENT_REPOSITORY_ALIAS,
	name: 'Bulk Duplicate Media Repository',
	api: () => import('./duplicate-to.repository.js'),
};

export const manifests: Array<ManifestTypes> = [bulkDuplicateRepository];
