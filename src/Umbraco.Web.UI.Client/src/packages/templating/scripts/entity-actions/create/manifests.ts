import { UMB_SCRIPT_FOLDER_ENTITY_TYPE, UMB_SCRIPT_ROOT_ENTITY_TYPE } from '../../entity.js';
import { manifests as defaultManifests } from './default/manifests.js';
import { manifests as folderManifests } from './folder/manifests.js';
import { UMB_IS_SERVER_PRODUCTION_MODE_CONDITION_ALIAS } from '@umbraco-cms/backoffice/server';
import type { UmbExtensionManifestKind } from '@umbraco-cms/backoffice/extension-registry';

export const manifests: Array<UmbExtensionManifest | UmbExtensionManifestKind> = [
	{
		type: 'entityAction',
		kind: 'create',
		alias: 'Umb.EntityAction.Script.Create',
		name: 'Create Script Entity Action',
		forEntityTypes: [UMB_SCRIPT_ROOT_ENTITY_TYPE, UMB_SCRIPT_FOLDER_ENTITY_TYPE],
		conditions: [
			{
				alias: UMB_IS_SERVER_PRODUCTION_MODE_CONDITION_ALIAS,
				match: false,
			},
		],
	},
	...defaultManifests,
	...folderManifests,
	// Deprecated: kept for backwards compatibility. Scheduled for removal in Umbraco 19.
	{
		type: 'modal',
		alias: 'Umb.Modal.Script.CreateOptions',
		name: 'Script Create Options Modal',
		element: () => import('./options-modal/script-create-options-modal.element.js'),
	},
];
