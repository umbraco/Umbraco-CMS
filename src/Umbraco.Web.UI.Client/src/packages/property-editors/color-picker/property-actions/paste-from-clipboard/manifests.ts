import { UMB_WRITABLE_PROPERTY_CONDITION_ALIAS } from '@umbraco-cms/backoffice/property';
import { UMB_COLOR_PICKER_PROPERTY_EDITOR_UI_ALIAS } from '../../constants.js';

export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'propertyAction',
		kind: 'default',
		alias: 'Umb.PropertyAction.ColorPicker.PasteFromClipboard',
		name: 'Color Picker Paste From Clipboard Property Action',
		api: () => import('./paste-from-clipboard.property-action.js'),
		forPropertyEditorUis: [UMB_COLOR_PICKER_PROPERTY_EDITOR_UI_ALIAS],
		meta: {
			icon: 'icon-paste-in',
			label: 'Paste from clipboard',
		},
		conditions: [
			{
				alias: UMB_WRITABLE_PROPERTY_CONDITION_ALIAS,
			},
		],
	},
];
