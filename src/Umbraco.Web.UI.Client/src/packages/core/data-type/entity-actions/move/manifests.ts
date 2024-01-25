import { UMB_DATA_TYPE_ENTITY_TYPE } from '../../entity.js';
import { UMB_MOVE_DATA_TYPE_REPOSITORY_ALIAS } from '../../repository/move/manifests.js';
import { UmbMoveDataTypeEntityAction } from './move.action.js';
import type { ManifestTypes } from '@umbraco-cms/backoffice/extension-registry';

const entityActions: Array<ManifestTypes> = [
	{
		type: 'entityAction',
		alias: 'Umb.EntityAction.DataType.Move',
		name: 'Move Data Type Entity Action',
		weight: 900,
		api: UmbMoveDataTypeEntityAction,
		meta: {
			icon: 'icon-enter',
			label: 'Move to...',
			repositoryAlias: UMB_MOVE_DATA_TYPE_REPOSITORY_ALIAS,
			entityTypes: [UMB_DATA_TYPE_ENTITY_TYPE],
		},
	},
];

export const manifests = [...entityActions];
