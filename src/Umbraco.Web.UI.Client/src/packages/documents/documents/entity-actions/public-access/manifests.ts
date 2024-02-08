//import { UMB_DOCUMENT_REPOSITORY_ALIAS } from '../../repository/manifests.js';
import { UMB_DOCUMENT_ENTITY_TYPE } from '../../entity.js';
import { UMB_DOCUMENT_PUBLIC_ACCESS_REPOSITORY_ALIAS } from './repository/manifests.js';
import { UmbDocumentPublicAccessEntityAction } from './public-access.action.js';
import type { ManifestEntityAction, ManifestModal } from '@umbraco-cms/backoffice/extension-registry';

const entityActions: Array<ManifestEntityAction> = [
	{
		type: 'entityAction',
		alias: 'Umb.EntityAction.Document.PublicAccess',
		name: 'Document Permissions Entity Action',
		api: UmbDocumentPublicAccessEntityAction,
		meta: {
			icon: 'icon-lock',
			label: 'Restrict Public Access',
			repositoryAlias: UMB_DOCUMENT_PUBLIC_ACCESS_REPOSITORY_ALIAS,
			entityTypes: [UMB_DOCUMENT_ENTITY_TYPE],
		},
	},
];

const manifestModals: Array<ManifestModal> = [
	{
		type: 'modal',
		alias: 'Umb.Modal.PublicAccess',
		name: 'Public Access Modal',
		js: () => import('./modal/public-access-modal.element.js'),
	},
];

export const manifests = [...entityActions, ...manifestModals];
