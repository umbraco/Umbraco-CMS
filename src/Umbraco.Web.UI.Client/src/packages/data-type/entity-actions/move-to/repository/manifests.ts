import { UMB_MOVE_DATA_TYPE_REPOSITORY_ALIAS } from './constants.js';
import { UmbMoveDataTypeRepository } from './data-type-move.repository.js';
import type { ManifestRepository } from '@umbraco-cms/backoffice/extension-registry';

const moveRepository: ManifestRepository = {
	type: 'repository',
	alias: UMB_MOVE_DATA_TYPE_REPOSITORY_ALIAS,
	name: 'Move Data Type Repository',
	api: UmbMoveDataTypeRepository,
};

export const manifests = [moveRepository];
