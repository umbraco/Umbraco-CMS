import { DATA_TYPE_REPOSITORY_ALIAS } from '../../repository/manifests';
import { UmbCreateDataTypeEntityAction } from './create.action';
import { ManifestTypes } from '@umbraco-cms/backoffice/extensions-registry';

const entityActions: Array<ManifestTypes> = [
	{
		type: 'entityAction',
		alias: 'Umb.EntityAction.DataType.Create',
		name: 'Create Data Type Entity Action',
		weight: 1000,
		meta: {
			icon: 'umb:add',
			label: 'Create',
			repositoryAlias: DATA_TYPE_REPOSITORY_ALIAS,
			api: UmbCreateDataTypeEntityAction,
		},
		conditions: {
			entityType: 'data-type',
		},
	},
	{
		type: 'modal',
		alias: 'Umb.Modal.DataTypeCreateOptions',
		name: 'Data Type Create Options Modal',
		loader: () => import('./modal/data-type-create-options-modal.element'),
	},
];

export const manifests = [...entityActions];
