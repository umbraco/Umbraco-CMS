import { UMB_COLOR_PICKER_PROPERTY_EDITOR_UI_ALIAS } from '../../constants.js';
import {
	UMB_PROPERTY_HAS_VALUE_CONDITION_ALIAS,
	UMB_WRITABLE_PROPERTY_CONDITION_ALIAS,
} from '@umbraco-cms/backoffice/property';

export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'propertyAction',
		kind: 'copyToClipboard',
		alias: 'Umb.PropertyAction.ColorPicker.CopyToClipboard',
		name: 'Color Picker Copy To Clipboard Property Action',
		forPropertyEditorUis: [UMB_COLOR_PICKER_PROPERTY_EDITOR_UI_ALIAS],
		meta: {},
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
		type: 'clipboardCopyTranslator',
		alias: 'Umb.ClipboardCopyTranslator.ColorPickerToColor',
		name: 'Color Picker To Color Clipboard Copy Translator',
		api: () => import('./color-picker-to-color-copy-translator.js'),
		fromPropertyEditorUi: UMB_COLOR_PICKER_PROPERTY_EDITOR_UI_ALIAS,
		toClipboardEntryValueType: 'color',
	},
];
