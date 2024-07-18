import { UMB_BULK_TRASH_DOCUMENT_REPOSITORY_ALIAS } from './constants.js';
import type { ManifestRepository, ManifestTypes } from '@umbraco-cms/backoffice/extension-registry';

const bulkTrashRepository: ManifestRepository = {
	type: 'repository',
	alias: UMB_BULK_TRASH_DOCUMENT_REPOSITORY_ALIAS,
	name: 'Bulk Trash Document Repository',
	api: () => import('./trash.repository.js'),
};

export const manifests: Array<ManifestTypes> = [bulkTrashRepository];
