import { UMB_ELEMENT_FOLDER_ENTITY_TYPE, UMB_ELEMENT_ROOT_ENTITY_TYPE } from '../../entity.js';
import { UMB_USER_PERMISSION_ELEMENT_FOLDER_CREATE } from '../../folder/user-permissions/constants.js';
import {
	UMB_ELEMENT_OR_ELEMENT_FOLDER_USER_PERMISSION_CONDITION_ALIAS,
	UMB_USER_PERMISSION_ELEMENT_CREATE,
} from '../../user-permissions/constants.js';
import { UMB_ENTITY_IS_NOT_TRASHED_CONDITION_ALIAS } from '@umbraco-cms/backoffice/recycle-bin';

export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'entityAction',
		kind: 'default',
		alias: 'Umb.EntityAction.Element.Create',
		name: 'Create Element Entity Action',
		weight: 1200,
		api: () => import('./create.action.js'),
		forEntityTypes: [UMB_ELEMENT_ROOT_ENTITY_TYPE, UMB_ELEMENT_FOLDER_ENTITY_TYPE],
		meta: {
			icon: 'icon-add',
			label: '#actions_createFor',
			additionalOptions: true,
		},
		conditions: [
			{
				alias: UMB_ELEMENT_OR_ELEMENT_FOLDER_USER_PERMISSION_CONDITION_ALIAS,
				element: { allOf: [UMB_USER_PERMISSION_ELEMENT_CREATE] },
				folder: { allOf: [UMB_USER_PERMISSION_ELEMENT_FOLDER_CREATE] },
			},
			{
				alias: UMB_ENTITY_IS_NOT_TRASHED_CONDITION_ALIAS,
			},
		],
	},
	{
		type: 'modal',
		alias: 'Umb.Modal.Element.CreateOptions',
		name: 'Element Create Options Modal',
		element: () => import('./element-create-options-modal.element.js'),
	},
];
