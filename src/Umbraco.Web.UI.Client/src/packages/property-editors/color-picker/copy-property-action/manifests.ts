import { UMB_WRITABLE_PROPERTY_CONDITION_ALIAS } from '@umbraco-cms/backoffice/property';
import { UMB_COLOR_PICKER_PROPERTY_EDITOR_UI_ALIAS } from '../constants.js';

export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'propertyAction',
		kind: 'default',
		alias: 'Umb.PropertyAction.ColorPicker.Copy',
		name: 'Copy Color Picker Property Action',
		api: () => import('./copy.property-action.js'),
		forPropertyEditorUis: [UMB_COLOR_PICKER_PROPERTY_EDITOR_UI_ALIAS],
		meta: {
			icon: 'icon-paste-in',
			label: 'Copy',
		},
		conditions: [
			{
				alias: UMB_WRITABLE_PROPERTY_CONDITION_ALIAS,
			},
		],
	},
];
