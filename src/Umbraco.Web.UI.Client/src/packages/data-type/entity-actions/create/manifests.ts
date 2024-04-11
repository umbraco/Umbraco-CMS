import { UMB_DATA_TYPE_FOLDER_ENTITY_TYPE, UMB_DATA_TYPE_ROOT_ENTITY_TYPE } from '../../entity.js';
import { UmbCreateDataTypeEntityAction } from './create.action.js';
import type { ManifestTypes } from '@umbraco-cms/backoffice/extension-registry';

const entityActions: Array<ManifestTypes> = [
	{
		type: 'entityAction',
		kind: 'default',
		alias: 'Umb.EntityAction.DataType.Create',
		name: 'Create Data Type Entity Action',
		weight: 1000,
		api: UmbCreateDataTypeEntityAction,
		forEntityTypes: [UMB_DATA_TYPE_ROOT_ENTITY_TYPE, UMB_DATA_TYPE_FOLDER_ENTITY_TYPE],
		meta: {
			icon: 'icon-add',
			label: '#actions_create',
		},
	},
	{
		type: 'modal',
		alias: 'Umb.Modal.DataTypeCreateOptions',
		name: 'Data Type Create Options Modal',
		element: () => import('./modal/data-type-create-options-modal.element.js'),
	},
];

export const manifests = [...entityActions];
