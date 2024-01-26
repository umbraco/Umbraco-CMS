import { UMB_DOCUMENT_DETAIL_REPOSITORY_ALIAS } from '../../repository/index.js';
import { UMB_DOCUMENT_ENTITY_TYPE } from '../../entity.js';
import { UmbDocumentPermissionsEntityAction } from './permissions.action.js';
import type { ManifestEntityAction, ManifestModal } from '@umbraco-cms/backoffice/extension-registry';

const entityActions: Array<ManifestEntityAction> = [
	{
		type: 'entityAction',
		alias: 'Umb.EntityAction.Document.Permissions',
		name: 'Document Permissions Entity Action',
		api: UmbDocumentPermissionsEntityAction,
		meta: {
			icon: 'icon-vcard',
			label: 'Permissions (TBD)',
			repositoryAlias: UMB_DOCUMENT_DETAIL_REPOSITORY_ALIAS,
			entityTypes: [UMB_DOCUMENT_ENTITY_TYPE],
		},
	},
];

const modals: Array<ManifestModal> = [
	{
		type: 'modal',
		alias: 'Umb.Modal.Permissions',
		name: 'Permissions Modal',
		js: () => import('./permissions-modal.element.js'),
	},
];

export const manifests = [...entityActions, ...modals];
