import { UMB_DOCUMENT_BLUEPRINT_FOLDER_ENTITY_TYPE, UMB_DOCUMENT_BLUEPRINT_ROOT_ENTITY_TYPE } from '../../entity.js';

export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'entityAction',
		kind: 'default',
		alias: 'Umb.EntityAction.DocumentBlueprint.Create',
		name: 'Document Blueprint Options Create Entity Action',
		weight: 1200,
		api: () => import('./create.action.js'),
		forEntityTypes: [UMB_DOCUMENT_BLUEPRINT_ROOT_ENTITY_TYPE, UMB_DOCUMENT_BLUEPRINT_FOLDER_ENTITY_TYPE],
		meta: {
			icon: 'icon-add',
			label: '#actions_createblueprint',
			additionalOptions: true,
		},
	},
	{
		type: 'modal',
		alias: 'Umb.Modal.DocumentBlueprintOptionsCreate',
		name: 'Document Blueprint Options Create Modal',
		element: () => import('./modal/document-blueprint-options-create-modal.element.js'),
	},
];
