import { UMB_COLOR_PICKER_PROPERTY_EDITOR_UI_ALIAS } from '../../constants.js';
import { UMB_WRITABLE_PROPERTY_CONDITION_ALIAS } from '@umbraco-cms/backoffice/property';

export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'propertyAction',
		kind: 'pasteFromClipboard',
		alias: 'Umb.PropertyAction.ColorPicker.PasteFromClipboard',
		name: 'Color Picker Paste From Clipboard Property Action',
		forPropertyEditorUis: [UMB_COLOR_PICKER_PROPERTY_EDITOR_UI_ALIAS],
		meta: {},
		conditions: [
			{
				alias: UMB_WRITABLE_PROPERTY_CONDITION_ALIAS,
			},
		],
	},
];
