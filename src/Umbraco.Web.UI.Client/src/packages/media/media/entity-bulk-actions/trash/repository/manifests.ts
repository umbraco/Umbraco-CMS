import { UMB_BULK_TRASH_MEDIA_REPOSITORY_ALIAS } from './constants.js';
import type { ManifestRepository, ManifestTypes } from '@umbraco-cms/backoffice/extension-registry';

const bulkTrashRepository: ManifestRepository = {
	type: 'repository',
	alias: UMB_BULK_TRASH_MEDIA_REPOSITORY_ALIAS,
	name: 'Bulk Trash Media Repository',
	api: () => import('./trash.repository.js'),
};

export const manifests: Array<ManifestTypes> = [bulkTrashRepository];
