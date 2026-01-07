import { UMB_ELEMENT_FOLDER_ENTITY_TYPE } from '../../tree/folder/entity.js';
import { UMB_ELEMENT_ROOT_ENTITY_TYPE } from '../../entity.js';
import { UMB_ELEMENT_FOLDER_REPOSITORY_ALIAS } from '../../tree/folder/constants.js';
import { UMB_ENTITY_IS_NOT_TRASHED_CONDITION_ALIAS } from '@umbraco-cms/backoffice/recycle-bin';
import type { ManifestEntityAction } from '@umbraco-cms/backoffice/entity-action';
import type { ManifestModal } from '@umbraco-cms/backoffice/modal';

const entityActions: Array<ManifestEntityAction> = [
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
			// {
			// 	alias: 'Umb.Condition.UserPermission.Element',
			// 	allOf: [UMB_USER_PERMISSION_ELEMENT_CREATE],
			// },
			{
				alias: UMB_ENTITY_IS_NOT_TRASHED_CONDITION_ALIAS,
			},
		],
	},
	{
		type: 'entityAction',
		kind: 'folderCreate',
		alias: 'Umb.EntityAction.Element.CreateFolder',
		name: 'Create Element Folder Entity Action',
		weight: 1100,
		forEntityTypes: [UMB_ELEMENT_ROOT_ENTITY_TYPE, UMB_ELEMENT_FOLDER_ENTITY_TYPE],
		meta: {
			folderRepositoryAlias: UMB_ELEMENT_FOLDER_REPOSITORY_ALIAS,
		},
		conditions: [
			{
				alias: UMB_ENTITY_IS_NOT_TRASHED_CONDITION_ALIAS,
			},
		],
	},
];

const modal: ManifestModal = {
	type: 'modal',
	alias: 'Umb.Modal.Element.CreateOptions',
	name: 'Element Create Options Modal',
	element: () => import('./element-create-options-modal.element.js'),
};

export const manifests: Array<UmbExtensionManifest> = [...entityActions, modal];
