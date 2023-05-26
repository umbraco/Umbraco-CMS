import { DATA_TYPE_ENTITY_TYPE } from '../../index.js';
import { DATA_TYPE_REPOSITORY_ALIAS } from '../../repository/manifests.js';
import { UmbMoveDataTypeEntityAction } from './move.action.js';
import { ManifestTypes } from '@umbraco-cms/backoffice/extension-registry';

const entityActions: Array<ManifestTypes> = [
	{
		type: 'entityAction',
		alias: 'Umb.EntityAction.DataType.Move',
		name: 'Move Data Type Entity Action',
		weight: 900,
		meta: {
			icon: 'umb:enter',
			label: 'Move to...',
			repositoryAlias: DATA_TYPE_REPOSITORY_ALIAS,
			api: UmbMoveDataTypeEntityAction,
		},
		conditions: {
			entityTypes: [DATA_TYPE_ENTITY_TYPE],
		},
	},
];

export const manifests = [...entityActions];
