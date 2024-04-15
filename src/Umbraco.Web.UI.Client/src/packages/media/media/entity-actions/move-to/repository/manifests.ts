import { UMB_MOVE_MEDIA_REPOSITORY_ALIAS } from './constants.js';
import { UmbMoveMediaRepository } from './media-move.repository.js';
import type { ManifestRepository } from '@umbraco-cms/backoffice/extension-registry';

const moveRepository: ManifestRepository = {
	type: 'repository',
	alias: UMB_MOVE_MEDIA_REPOSITORY_ALIAS,
	name: 'Move Media Repository',
	api: UmbMoveMediaRepository,
};

export const manifests = [moveRepository];
