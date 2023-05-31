import { DATA_TYPE_ENTITY_TYPE } from '../../index.js';
import { DATA_TYPE_REPOSITORY_ALIAS } from '../../repository/manifests.js';
import { UmbCopyDataTypeEntityAction } from './copy.action.js';
import { ManifestTypes } from '@umbraco-cms/backoffice/extension-registry';

const entityActions: Array<ManifestTypes> = [
	{
		type: 'entityAction',
		alias: 'Umb.EntityAction.DataType.Copy',
		name: 'Copy Data Type Entity Action',
		weight: 900,
		meta: {
			icon: 'umb:documents',
			label: 'Copy to...',
			repositoryAlias: DATA_TYPE_REPOSITORY_ALIAS,
			api: UmbCopyDataTypeEntityAction,
		},
		conditions: {
			entityTypes: [DATA_TYPE_ENTITY_TYPE],
		},
	},
];

export const manifests = [...entityActions];
