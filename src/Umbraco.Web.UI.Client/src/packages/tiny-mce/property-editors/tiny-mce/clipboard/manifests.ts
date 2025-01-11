import { UMB_TINY_MCE_PROPERTY_EDITOR_UI_ALIAS } from '../constants.js';
import { manifests as blockPasteManifests } from './block/paste/manifests.js';
import {
	UMB_PROPERTY_HAS_VALUE_CONDITION_ALIAS,
	UMB_WRITABLE_PROPERTY_CONDITION_ALIAS,
} from '@umbraco-cms/backoffice/property';

export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'propertyAction',
		kind: 'copyToClipboard',
		alias: 'Umb.PropertyAction.TinyMce.Clipboard.Copy',
		name: 'Tiny Mce Copy To Clipboard Property Action',
		forPropertyEditorUis: [UMB_TINY_MCE_PROPERTY_EDITOR_UI_ALIAS],
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
		alias: 'Umb.PropertyAction.TinyMce.Clipboard.Paste',
		name: 'Tiny Mce Paste From Clipboard Property Action',
		forPropertyEditorUis: [UMB_TINY_MCE_PROPERTY_EDITOR_UI_ALIAS],
		conditions: [
			{
				alias: UMB_WRITABLE_PROPERTY_CONDITION_ALIAS,
			},
		],
	},
	...blockPasteManifests,
];
