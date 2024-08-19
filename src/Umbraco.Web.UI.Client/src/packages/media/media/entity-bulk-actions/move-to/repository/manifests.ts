import { UMB_BULK_MOVE_MEDIA_REPOSITORY_ALIAS } from './constants.js';
import type { ManifestRepository, ManifestTypes } from '@umbraco-cms/backoffice/extension-registry';

const bulkMoveRepository: ManifestRepository = {
	type: 'repository',
	alias: UMB_BULK_MOVE_MEDIA_REPOSITORY_ALIAS,
	name: 'Bulk Move Media Repository',
	api: () => import('./move-to.repository.js'),
};

export const manifests: Array<ManifestTypes> = [bulkMoveRepository];
