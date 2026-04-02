import { UMB_DOCUMENT_BLUEPRINT_FOLDER_ENTITY_TYPE, UMB_DOCUMENT_BLUEPRINT_ROOT_ENTITY_TYPE } from '../../entity.js';
import { manifests as defaultManifests } from './default/manifests.js';
import { manifests as folderManifests } from './folder/manifests.js';
import type { UmbExtensionManifestKind } from '@umbraco-cms/backoffice/extension-registry';

export const manifests: Array<UmbExtensionManifest | UmbExtensionManifestKind> = [
	{
		type: 'entityAction',
		kind: 'create',
		alias: 'Umb.EntityAction.DocumentBlueprint.Create',
		name: 'Create Document Blueprint Entity Action',
		forEntityTypes: [UMB_DOCUMENT_BLUEPRINT_ROOT_ENTITY_TYPE, UMB_DOCUMENT_BLUEPRINT_FOLDER_ENTITY_TYPE],
	},
	...defaultManifests,
	...folderManifests,
	// Deprecated: kept for backwards compatibility. Scheduled for removal in Umbraco 19.
	{
		type: 'modal',
		alias: 'Umb.Modal.DocumentBlueprintOptionsCreate',
		name: 'Document Blueprint Options Create Modal',
		element: () => import('./modal/document-blueprint-options-create-modal.element.js'),
	},
];
