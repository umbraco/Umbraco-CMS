import { UMB_DOCUMENT_ENTITY_TYPE } from '../../entity.js';
import { UMB_USER_PERMISSION_DOCUMENT_CREATE_BLUEPRINT } from '../../user-permissions/constants.js';
import type { ManifestTypes } from '@umbraco-cms/backoffice/extension-registry';

const entityActions: Array<ManifestTypes> = [
	{
		type: 'entityAction',
		kind: 'default',
		alias: 'Umb.EntityAction.Document.CreateBlueprint',
		name: 'Create Document Blueprint Entity Action',
		weight: 1000,
		api: () => import('./create-blueprint.action.js'),
		forEntityTypes: [UMB_DOCUMENT_ENTITY_TYPE],
		meta: {
			icon: 'icon-blueprint',
			label: '#actions_createblueprint',
		},
		conditions: [
			{
				alias: 'Umb.Condition.UserPermission.Document',
				allOf: [UMB_USER_PERMISSION_DOCUMENT_CREATE_BLUEPRINT],
			},
		],
	},
];

const manifestModals: Array<ManifestTypes> = [
	{
		type: 'modal',
		alias: 'Umb.Modal.CreateBlueprint',
		name: 'Create Blueprint Modal',
		js: () => import('./modal/create-blueprint-modal.element.js'),
	},
];

export const manifests: Array<ManifestTypes> = [...entityActions, ...manifestModals];
