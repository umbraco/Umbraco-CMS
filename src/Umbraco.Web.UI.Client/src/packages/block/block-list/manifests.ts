import { manifests as clipboardManifests } from './clipboard/manifests.js';
import { UMB_BLOCK_LIST_PROPERTY_EDITOR_UI_ALIAS } from './constants.js';
import { manifests as propertyEditorManifests } from './property-editors/manifests.js';
import { manifests as propertyValueClonerManifests } from './property-value-cloner/manifests.js';
import { manifests as validationManifests } from './validation/manifests.js';
import { manifests as workspaceManifests } from './workspace/manifests.js';
import type { UmbExtensionManifestKind } from '@umbraco-cms/backoffice/extension-registry';

export const manifests: Array<UmbExtensionManifest | UmbExtensionManifestKind> = [
	...clipboardManifests,
	...propertyEditorManifests,
	...propertyValueClonerManifests,
	...validationManifests,
	...workspaceManifests,
	{
		type: 'propertyAction',
		kind: 'clear',
		alias: 'Umb.PropertyAction.BlockList.Clear',
		name: 'Clear Block List Property Action',
		forPropertyEditorUis: [UMB_BLOCK_LIST_PROPERTY_EDITOR_UI_ALIAS],
	},
];
