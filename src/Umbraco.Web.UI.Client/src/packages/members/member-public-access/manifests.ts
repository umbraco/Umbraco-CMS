import { UMB_SECTION_USER_PERMISSION_CONDITION_ALIAS } from '@umbraco-cms/backoffice/section';
import { UMB_ENTITY_IS_NOT_TRASHED_CONDITION_ALIAS } from '@umbraco-cms/backoffice/recycle-bin';
import { UMB_DOCUMENT_ENTITY_TYPE, UMB_USER_PERMISSION_DOCUMENT_PUBLIC_ACCESS } from '@umbraco-cms/backoffice/document';
import { UMB_MEMBER_MANAGEMENT_SECTION_ALIAS } from '../section/constants.js';

export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'entityAction',
		kind: 'default',
		alias: 'Umb.EntityAction.Document.PublicAccess',
		name: 'Document Public Access Entity Action',
		weight: 200,
		api: () => import('./public-access.action.js'),
		forEntityTypes: [UMB_DOCUMENT_ENTITY_TYPE],
		meta: {
			icon: 'icon-lock',
			label: '#actions_protect',
			additionalOptions: true,
		},
		conditions: [
			{
				alias: 'Umb.Condition.UserPermission.Document',
				allOf: [UMB_USER_PERMISSION_DOCUMENT_PUBLIC_ACCESS],
			},
			{
				alias: UMB_ENTITY_IS_NOT_TRASHED_CONDITION_ALIAS,
			},
			{
				alias: UMB_SECTION_USER_PERMISSION_CONDITION_ALIAS,
				match: UMB_MEMBER_MANAGEMENT_SECTION_ALIAS,
			},
		],
	},
	{
		type: 'modal',
		alias: 'Umb.Modal.PublicAccess',
		name: 'Public Access Modal',
		element: () => import('./modal/public-access-modal.element.js'),
	},
];
