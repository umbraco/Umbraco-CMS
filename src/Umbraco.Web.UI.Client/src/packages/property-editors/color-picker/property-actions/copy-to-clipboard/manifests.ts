import { UMB_WRITABLE_PROPERTY_CONDITION_ALIAS } from '@umbraco-cms/backoffice/property';
import { UMB_COLOR_PICKER_PROPERTY_EDITOR_UI_ALIAS } from '../../constants.js';

export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'propertyAction',
		kind: 'default',
		alias: 'Umb.PropertyAction.ColorPicker.CopyToClipboard',
		name: 'Color Picker Copy To Clipboard Property Action',
		api: () => import('./copy-to-clipboard.property-action.js'),
		forPropertyEditorUis: [UMB_COLOR_PICKER_PROPERTY_EDITOR_UI_ALIAS],
		meta: {
			icon: 'icon-paste-in',
			label: 'Copy to clipboard',
		},
		conditions: [
			{
				alias: UMB_WRITABLE_PROPERTY_CONDITION_ALIAS,
			},
		],
	},
];
