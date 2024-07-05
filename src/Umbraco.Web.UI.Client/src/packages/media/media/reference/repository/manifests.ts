import { UMB_MEDIA_REFERENCE_REPOSITORY_ALIAS } from './constants.js';
import type { ManifestRepository, ManifestTypes } from '@umbraco-cms/backoffice/extension-registry';

const repository: ManifestRepository = {
	type: 'repository',
	alias: UMB_MEDIA_REFERENCE_REPOSITORY_ALIAS,
	name: 'Media Reference Repository',
	api: () => import('./media-reference.repository.js'),
};

export const manifests: Array<ManifestTypes> = [repository];
