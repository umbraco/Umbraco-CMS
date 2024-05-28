import { UMB_DOCUMENT_ENTITY_TYPE } from '../../entity.js';
import { UMB_USER_PERMISSION_DOCUMENT_PUBLIC_ACCESS } from '../../user-permissions/index.js';
import { UmbDocumentPublicAccessEntityAction } from './public-access.action.js';
import { UMB_ENTITY_IS_NOT_TRASHED_CONDITION_ALIAS } from '@umbraco-cms/backoffice/recycle-bin';
import type { ManifestTypes } from '@umbraco-cms/backoffice/extension-registry';

const entityActions: Array<ManifestTypes> = [
	{
		type: 'entityAction',
		kind: 'default',
		alias: 'Umb.EntityAction.Document.PublicAccess',
		name: 'Document Public Access Entity Action',
		weight: 200,
		api: UmbDocumentPublicAccessEntityAction,
		forEntityTypes: [UMB_DOCUMENT_ENTITY_TYPE],
		meta: {
			icon: 'icon-lock',
			label: '#actions_protect',
		},
		conditions: [
			{
				alias: 'Umb.Condition.UserPermission.Document',
				allOf: [UMB_USER_PERMISSION_DOCUMENT_PUBLIC_ACCESS],
			},
			{
				alias: UMB_ENTITY_IS_NOT_TRASHED_CONDITION_ALIAS,
			},
		],
	},
];

const manifestModals: Array<ManifestTypes> = [
	{
		type: 'modal',
		alias: 'Umb.Modal.PublicAccess',
		name: 'Public Access Modal',
		js: () => import('./modal/public-access-modal.element.js'),
	},
];

export const manifests: Array<ManifestTypes> = [...entityActions, ...manifestModals];
