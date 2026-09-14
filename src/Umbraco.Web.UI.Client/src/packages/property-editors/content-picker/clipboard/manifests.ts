import { manifests as mediaManifests } from './media/manifests.js';
import { UMB_CONTENT_PICKER_SOURCE_TYPE_CONDITION_ALIAS } from '../conditions/constants.js';
import {
	UMB_PROPERTY_HAS_VALUE_CONDITION_ALIAS,
	UMB_WRITABLE_PROPERTY_CONDITION_ALIAS,
} from '@umbraco-cms/backoffice/property';

const forPropertyEditorUis = ['Umb.PropertyEditorUi.ContentPicker'];

export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'propertyContext',
		kind: 'clipboard',
		alias: 'Umb.PropertyContext.ContentPicker.Clipboard',
		name: 'Content Picker Clipboard Property Context',
		forPropertyEditorUis,
	},
	{
		type: 'propertyAction',
		kind: 'copyToClipboard',
		alias: 'Umb.PropertyAction.ContentPicker.Clipboard.Copy',
		name: 'Content Picker Copy To Clipboard Property Action',
		forPropertyEditorUis,
		conditions: [
			{
				alias: UMB_PROPERTY_HAS_VALUE_CONDITION_ALIAS,
			},
			// Only a Content Picker that picks media has media to copy — the clipboard translators are media only.
			{
				alias: UMB_CONTENT_PICKER_SOURCE_TYPE_CONDITION_ALIAS,
				match: 'media',
			},
		],
	},
	{
		type: 'propertyAction',
		kind: 'pasteFromClipboard',
		alias: 'Umb.PropertyAction.ContentPicker.Clipboard.Paste',
		name: 'Content Picker Paste From Clipboard Property Action',
		forPropertyEditorUis,
		conditions: [
			{
				alias: UMB_WRITABLE_PROPERTY_CONDITION_ALIAS,
			},
			{
				alias: UMB_CONTENT_PICKER_SOURCE_TYPE_CONDITION_ALIAS,
				match: 'media',
			},
		],
	},
	...mediaManifests,
];
