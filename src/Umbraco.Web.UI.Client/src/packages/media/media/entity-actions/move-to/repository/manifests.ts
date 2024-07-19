import { UMB_MOVE_MEDIA_REPOSITORY_ALIAS } from './constants.js';
import type { ManifestRepository, ManifestTypes } from '@umbraco-cms/backoffice/extension-registry';

const moveRepository: ManifestRepository = {
	type: 'repository',
	alias: UMB_MOVE_MEDIA_REPOSITORY_ALIAS,
	name: 'Move Media Repository',
	api: () => import('./media-move.repository.js'),
};

export const manifests: Array<ManifestTypes> = [moveRepository];
