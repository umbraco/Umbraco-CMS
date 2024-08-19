import { UMB_EXPORT_MEDIA_TYPE_REPOSITORY_ALIAS } from './constants.js';
import type { ManifestRepository, ManifestTypes } from '@umbraco-cms/backoffice/extension-registry';

const exportRepository: ManifestRepository = {
	type: 'repository',
	alias: UMB_EXPORT_MEDIA_TYPE_REPOSITORY_ALIAS,
	name: 'Export Media Type Repository',
	api: () => import('./media-type-export.repository.js'),
};

export const manifests: Array<ManifestTypes> = [exportRepository];
