import { manifests as richMediaManifests } from './rich-media/manifests.js';
import { manifests as mediaManifests } from './media/manifests.js';
import {
	UMB_PROPERTY_HAS_VALUE_CONDITION_ALIAS,
	UMB_WRITABLE_PROPERTY_CONDITION_ALIAS,
} from '@umbraco-cms/backoffice/property';

const forPropertyEditorUis = ['Umb.PropertyEditorUi.MediaPicker'];

export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'propertyContext',
		kind: 'clipboard',
		alias: 'Umb.PropertyContext.MediaPicker.Clipboard',
		name: 'Media Picker Clipboard Property Context',
		forPropertyEditorUis,
	},
	{
		type: 'propertyAction',
		kind: 'copyToClipboard',
		alias: 'Umb.PropertyAction.MediaPicker.Clipboard.Copy',
		name: 'Media Picker Copy To Clipboard Property Action',
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
		alias: 'Umb.PropertyAction.MediaPicker.Clipboard.Paste',
		name: 'Media Picker Paste From Clipboard Property Action',
		forPropertyEditorUis,
		conditions: [
			{
				alias: UMB_WRITABLE_PROPERTY_CONDITION_ALIAS,
			},
		],
	},
	...richMediaManifests,
	...mediaManifests,
];
