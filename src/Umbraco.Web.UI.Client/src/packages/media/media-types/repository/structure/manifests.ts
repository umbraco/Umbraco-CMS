import { UMB_MEDIA_TYPE_STRUCTURE_REPOSITORY_ALIAS } from './constants.js';
import type { ManifestRepository, ManifestTypes } from '@umbraco-cms/backoffice/extension-registry';

const structureRepository: ManifestRepository = {
	type: 'repository',
	alias: UMB_MEDIA_TYPE_STRUCTURE_REPOSITORY_ALIAS,
	name: 'Media Type Structure Repository',
	api: () => import('./media-type-structure.repository.js'),
};

export const manifests: Array<ManifestTypes> = [structureRepository];
