import { UmbMoveDataTypeRepository } from './data-type-move.repository.js';
import type { ManifestRepository } from '@umbraco-cms/backoffice/extension-registry';

export const UMB_MOVE_DATA_TYPE_REPOSITORY_ALIAS = 'Umb.Repository.DataType.Move';

const moveRepository: ManifestRepository = {
	type: 'repository',
	alias: UMB_MOVE_DATA_TYPE_REPOSITORY_ALIAS,
	name: 'Move Data Type Repository',
	api: UmbMoveDataTypeRepository,
};

export const manifests = [moveRepository];
