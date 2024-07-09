import { UMB_IMAGING_REPOSITORY_ALIAS } from './constants.js';
import type { ManifestRepository, ManifestTypes } from '@umbraco-cms/backoffice/extension-registry';

const repository: ManifestRepository = {
	type: 'repository',
	alias: UMB_IMAGING_REPOSITORY_ALIAS,
	name: 'Imaging Repository',
	api: () => import('./imaging.repository.js'),
};

export const manifests: Array<ManifestTypes> = [repository];
