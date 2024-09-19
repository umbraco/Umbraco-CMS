import { UMB_DOCUMENT_TYPE_FOLDER_ENTITY_TYPE, UMB_DOCUMENT_TYPE_ROOT_ENTITY_TYPE } from '../../entity.js';

export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'entityAction',
		kind: 'default',
		alias: 'Umb.EntityAction.DocumentType.Create',
		name: 'Create Document Type Entity Action',
		weight: 1200,
		api: () => import('./create.action.js'),
		forEntityTypes: [UMB_DOCUMENT_TYPE_ROOT_ENTITY_TYPE, UMB_DOCUMENT_TYPE_FOLDER_ENTITY_TYPE],
		meta: {
			icon: 'icon-add',
			label: '#actions_create',
		},
	},
	{
		type: 'modal',
		alias: 'Umb.Modal.DocumentTypeCreateOptions',
		name: 'Document Type Create Options Modal',
		element: () => import('./modal/document-type-create-options-modal.element.js'),
	},
];
