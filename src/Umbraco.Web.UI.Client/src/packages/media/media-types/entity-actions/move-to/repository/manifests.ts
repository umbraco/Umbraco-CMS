import { UMB_MOVE_MEDIA_TYPE_REPOSITORY_ALIAS } from './constants.js';
import type { ManifestRepository, ManifestTypes } from '@umbraco-cms/backoffice/extension-registry';

const moveRepository: ManifestRepository = {
	type: 'repository',
	alias: UMB_MOVE_MEDIA_TYPE_REPOSITORY_ALIAS,
	name: 'Move Media Type Repository',
	api: () => import('./media-type-move.repository.js'),
};

export const manifests: Array<ManifestTypes> = [moveRepository];
