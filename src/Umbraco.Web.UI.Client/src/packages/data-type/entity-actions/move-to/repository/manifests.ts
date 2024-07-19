import { UMB_MOVE_DATA_TYPE_REPOSITORY_ALIAS } from './constants.js';
import type { ManifestRepository, ManifestTypes } from '@umbraco-cms/backoffice/extension-registry';

const moveRepository: ManifestRepository = {
	type: 'repository',
	alias: UMB_MOVE_DATA_TYPE_REPOSITORY_ALIAS,
	name: 'Move Data Type Repository',
	api: () => import('./data-type-move.repository.js'),
};

export const manifests: Array<ManifestTypes> = [moveRepository];
