import { UMB_BLOCK_GRID_PROPERTY_EDITOR_UI_ALIAS } from '../property-editors/constants.js';
import { manifests as blockManifests } from './block/manifests.js';
import { manifests as gridBlockManifests } from './grid-block/manifests.js';
import { manifests as pasteManifests } from './paste/manifests.js';
import type { UmbExtensionManifestKind } from '@umbraco-cms/backoffice/extension-registry';
import {
	UMB_PROPERTY_HAS_VALUE_CONDITION_ALIAS,
	UMB_WRITABLE_PROPERTY_CONDITION_ALIAS,
} from '@umbraco-cms/backoffice/property';

const forPropertyEditorUis = [UMB_BLOCK_GRID_PROPERTY_EDITOR_UI_ALIAS];

export const manifests: Array<UmbExtensionManifest | UmbExtensionManifestKind> = [
	{
		type: 'propertyContext',
		kind: 'clipboard',
		alias: 'Umb.PropertyContext.BlockGrid.Clipboard',
		name: 'Block Grid Clipboard Property Context',
		forPropertyEditorUis,
	},
	{
		type: 'propertyAction',
		kind: 'copyToClipboard',
		alias: 'Umb.PropertyAction.BlockGrid.Clipboard.Copy',
		name: 'Block Grid Copy To Clipboard Property Action',
		forPropertyEditorUis,
		conditions: [
			{
				alias: UMB_WRITABLE_PROPERTY_CONDITION_ALIAS,
			},
			{
				alias: UMB_PROPERTY_HAS_VALUE_CONDITION_ALIAS,
			},
		],
	},
	...pasteManifests,
	...blockManifests,
	...gridBlockManifests,
];
