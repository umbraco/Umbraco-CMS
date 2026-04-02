import { UMB_BLOCK_SINGLE_PROPERTY_EDITOR_UI_ALIAS } from '../property-editors/constants.js';
import { manifests as blockCopyManifests } from './block/copy/manifests.js';
import { manifests as blockPasteManifests } from './block/paste/manifests.js';
import {
	UMB_PROPERTY_HAS_VALUE_CONDITION_ALIAS,
	UMB_WRITABLE_PROPERTY_CONDITION_ALIAS,
} from '@umbraco-cms/backoffice/property';

const forPropertyEditorUis = [UMB_BLOCK_SINGLE_PROPERTY_EDITOR_UI_ALIAS];

export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'propertyContext',
		kind: 'clipboard',
		alias: 'Umb.PropertyContext.BlockSingle.Clipboard',
		name: 'Block Single Clipboard Property Context',
		forPropertyEditorUis,
	},
	{
		type: 'propertyAction',
		kind: 'copyToClipboard',
		alias: 'Umb.PropertyAction.BlockSingle.Clipboard.Copy',
		name: 'Block Single Copy To Clipboard Property Action',
		forPropertyEditorUis,
		conditions: [
			{
				alias: UMB_PROPERTY_HAS_VALUE_CONDITION_ALIAS,
			},
		],
	},
	{
		type: 'propertyAction',
		kind: 'pasteFromClipboard',
		alias: 'Umb.PropertyAction.BlockSingle.Clipboard.Paste',
		name: 'Block Single Paste From Clipboard Property Action',
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
