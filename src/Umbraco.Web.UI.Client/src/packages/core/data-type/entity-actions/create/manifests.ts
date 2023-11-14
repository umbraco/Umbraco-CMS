import { DATA_TYPE_ENTITY_TYPE, DATA_TYPE_FOLDER_ENTITY_TYPE, DATA_TYPE_ROOT_ENTITY_TYPE } from '../../entities.js';
import { DATA_TYPE_DETAIL_REPOSITORY_ALIAS } from '../../repository/detail/manifests.js';
import { UmbCreateDataTypeEntityAction } from './create.action.js';
import { ManifestTypes } from '@umbraco-cms/backoffice/extension-registry';

const entityActions: Array<ManifestTypes> = [
	{
		type: 'entityAction',
		alias: 'Umb.EntityAction.DataType.Create',
		name: 'Create Data Type Entity Action',
		weight: 1000,
		api: UmbCreateDataTypeEntityAction,
		meta: {
			icon: 'icon-add',
			label: 'Create...',
			repositoryAlias: DATA_TYPE_DETAIL_REPOSITORY_ALIAS,
			entityTypes: [DATA_TYPE_ENTITY_TYPE, DATA_TYPE_ROOT_ENTITY_TYPE, DATA_TYPE_FOLDER_ENTITY_TYPE],
		},
	},
	{
		type: 'modal',
		alias: 'Umb.Modal.DataTypeCreateOptions',
		name: 'Data Type Create Options Modal',
		loader: () => import('./modal/data-type-create-options-modal.element.js'),
	},
];

export const manifests = [...entityActions];
