import { DATA_TYPE_REPOSITORY_ALIAS } from '../../repository/manifests';
import { UmbCreateDataTypeEntityAction } from './create.action';
import { ManifestTypes } from '@umbraco-cms/backoffice/extensions-registry';

const entityActions: Array<ManifestTypes> = [
	{
		type: 'entityAction',
		alias: 'Umb.EntityAction.DataType.Create',
		name: 'Create Data Type Entity Action',
		weight: 900,
		meta: {
			icon: 'umb:add',
			label: 'Create',
			repositoryAlias: DATA_TYPE_REPOSITORY_ALIAS,
			api: UmbCreateDataTypeEntityAction,
		},
		conditions: {
			entityType: 'data-type-root',
		},
	},
	{
		type: 'modal',
		alias: 'Umb.Modal.CreateDataType',
		name: 'Create Data Type Modal',
		loader: () => import('./modal/create-data-type-modal.element'),
	},
];

export const manifests = [...entityActions];
