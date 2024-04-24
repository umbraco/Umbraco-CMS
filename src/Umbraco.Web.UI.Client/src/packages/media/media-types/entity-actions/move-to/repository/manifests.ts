import { UMB_MOVE_MEDIA_TYPE_REPOSITORY_ALIAS } from './constants.js';
import { UmbMoveMediaTypeRepository } from './media-type-move.repository.js';
import type { ManifestRepository } from '@umbraco-cms/backoffice/extension-registry';

const moveRepository: ManifestRepository = {
	type: 'repository',
	alias: UMB_MOVE_MEDIA_TYPE_REPOSITORY_ALIAS,
	name: 'Move Media Type Repository',
	api: UmbMoveMediaTypeRepository,
};

export const manifests: Array<ManifestTypes> = [moveRepository];
