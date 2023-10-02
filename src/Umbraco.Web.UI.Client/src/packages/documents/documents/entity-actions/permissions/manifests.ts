import { DOCUMENT_REPOSITORY_ALIAS } from '../../repository/manifests.js';
import { UmbDocumentPermissionsEntityAction } from './permissions.action.js';
import { ManifestEntityAction, ManifestModal } from '@umbraco-cms/backoffice/extension-registry';
import { DOCUMENT_ENTITY_TYPE } from '@umbraco-cms/backoffice/document';

const entityActions: Array<ManifestEntityAction> = [
	{
		type: 'entityAction',
		alias: 'Umb.EntityAction.Document.Permissions',
		name: 'Document Permissions Entity Action',
		meta: {
			icon: 'umb:vcard',
			label: 'Permissions (TBD)',
			repositoryAlias: DOCUMENT_REPOSITORY_ALIAS,
			api: UmbDocumentPermissionsEntityAction,
			entityTypes: [DOCUMENT_ENTITY_TYPE],
		},
	},
];

const modals: Array<ManifestModal> = [
	{
		type: 'modal',
		alias: 'Umb.Modal.Permissions',
		name: 'Permissions Modal',
		loader: () => import('./permissions-modal.element.js'),
	},
];

export const manifests = [...entityActions, ...modals];
