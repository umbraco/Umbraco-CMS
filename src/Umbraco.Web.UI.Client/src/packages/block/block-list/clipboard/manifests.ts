import { UMB_BLOCK_LIST_PROPERTY_EDITOR_UI_ALIAS } from '../property-editors/constants.js';
import { manifests as blockCopyManifests } from './block/copy/manifests.js';
import { manifests as blockPasteManifests } from './block/paste/manifests.js';
import {
	UMB_PROPERTY_HAS_VALUE_CONDITION_ALIAS,
	UMB_WRITABLE_PROPERTY_CONDITION_ALIAS,
} from '@umbraco-cms/backoffice/property';

const forPropertyEditorUis = [UMB_BLOCK_LIST_PROPERTY_EDITOR_UI_ALIAS];

export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'propertyContext',
		kind: 'clipboard',
		alias: 'Umb.PropertyContext.BlockList.Clipboard',
		name: 'Block List Clipboard Property Context',
		forPropertyEditorUis,
	},
	{
		type: 'propertyAction',
		kind: 'copyToClipboard',
		alias: 'Umb.PropertyAction.BlockList.Clipboard.Copy',
		name: 'Block List Copy To Clipboard Property Action',
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
	{
		type: 'propertyAction',
		kind: 'pasteFromClipboard',
		alias: 'Umb.PropertyAction.BlockList.Clipboard.Paste',
		name: 'Block List Paste From Clipboard Property Action',
		forPropertyEditorUis,
		conditions: [
			{
				alias: UMB_WRITABLE_PROPERTY_CONDITION_ALIAS,
			},
		],
	},
	...blockCopyManifests,
	...blockPasteManifests,
];
