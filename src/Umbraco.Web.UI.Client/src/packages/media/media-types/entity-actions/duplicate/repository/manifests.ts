import { UMB_DUPLICATE_MEDIA_TYPE_REPOSITORY_ALIAS } from './constants.js';
import type { ManifestRepository, ManifestTypes } from '@umbraco-cms/backoffice/extension-registry';

const duplicateRepository: ManifestRepository = {
	type: 'repository',
	alias: UMB_DUPLICATE_MEDIA_TYPE_REPOSITORY_ALIAS,
	name: 'Duplicate Media Type Repository',
	api: () => import('./media-type-duplicate.repository.js'),
};

export const manifests: Array<ManifestTypes> = [duplicateRepository];
