import { UMB_DOCUMENT_BLUEPRINT_ENTITY_TYPE, UMB_DOCUMENT_BLUEPRINT_FOLDER_ENTITY_TYPE } from '../../entity.js';
import type { ManifestTypes } from '@umbraco-cms/backoffice/extension-registry';

const entityActions: Array<ManifestTypes> = [
	{
		type: 'entityAction',
		kind: 'default',
		alias: 'Umb.EntityAction.DocumentBlueprint.Create',
		name: 'Document Blueprint Options Create Entity Action',
		api: () => import('./create.action.js'),
		forEntityTypes: [UMB_DOCUMENT_BLUEPRINT_ENTITY_TYPE, UMB_DOCUMENT_BLUEPRINT_FOLDER_ENTITY_TYPE],
		meta: {
			icon: 'icon-add',
			label: '#actions_createblueprint',
		},
	},
];

const manifestModals: Array<ManifestTypes> = [
	{
		type: 'modal',
		alias: 'Umb.Modal.DocumentBlueprintOptionsCreate',
		name: 'Document Blueprint Options Create Modal',
		js: () => import('./modal/document-blueprint-options-create-modal.element.js'),
	},
];

export const manifests = [...entityActions, ...manifestModals];
