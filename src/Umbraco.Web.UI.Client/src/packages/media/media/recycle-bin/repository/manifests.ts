import { UMB_MEDIA_RECYCLE_BIN_REPOSITORY_ALIAS } from './constants.js';
import type { ManifestRepository, ManifestTypes } from '@umbraco-cms/backoffice/extension-registry';

const repository: ManifestRepository = {
	type: 'repository',
	alias: UMB_MEDIA_RECYCLE_BIN_REPOSITORY_ALIAS,
	name: 'Media Recycle Bin Repository',
	api: () => import('./media-recycle-bin.repository.js'),
};

export const manifests: Array<ManifestTypes> = [repository];
