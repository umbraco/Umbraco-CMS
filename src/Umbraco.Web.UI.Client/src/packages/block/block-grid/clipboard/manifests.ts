import { UMB_BLOCK_GRID_PROPERTY_EDITOR_UI_ALIAS } from '../property-editors/constants.js';
import { manifests as blockManifests } from './block/manifests.js';
import { manifests as gridBlockManifests } from './grid-block/manifests.js';
import {
	UMB_PROPERTY_HAS_VALUE_CONDITION_ALIAS,
	UMB_WRITABLE_PROPERTY_CONDITION_ALIAS,
} from '@umbraco-cms/backoffice/property';

export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'propertyAction',
		kind: 'copyToClipboard',
		alias: 'Umb.PropertyAction.BlockGrid.Clipboard.Copy',
		name: 'Block Grid Copy To Clipboard Property Action',
		forPropertyEditorUis: [UMB_BLOCK_GRID_PROPERTY_EDITOR_UI_ALIAS],
		conditions: [
			{
				alias: UMB_WRITABLE_PROPERTY_CONDITION_ALIAS,
			},
			{
				alias: UMB_PROPERTY_HAS_VALUE_CONDITION_ALIAS,
			},
		],
	},
	{
		type: 'propertyAction',
		kind: 'pasteFromClipboard',
		alias: 'Umb.PropertyAction.BlockGrid.Clipboard.Paste',
		name: 'Block Grid Paste From Clipboard Property Action',
		forPropertyEditorUis: [UMB_BLOCK_GRID_PROPERTY_EDITOR_UI_ALIAS],
		conditions: [
			{
				alias: UMB_WRITABLE_PROPERTY_CONDITION_ALIAS,
			},
		],
	},
	...blockManifests,
	...gridBlockManifests,
];
