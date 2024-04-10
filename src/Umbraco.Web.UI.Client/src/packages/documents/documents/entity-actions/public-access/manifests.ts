import { UMB_DOCUMENT_ENTITY_TYPE } from '../../entity.js';
import { UmbDocumentPublicAccessEntityAction } from './public-access.action.js';
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

export const manifests = [...entityActions, ...manifestModals];
