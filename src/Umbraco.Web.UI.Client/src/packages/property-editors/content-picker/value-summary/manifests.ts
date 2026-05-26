import { UMB_CONTENT_PICKER_PROPERTY_EDITOR_VALUE_TYPE } from '../value-type/constants.js';

export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'valueSummary',
		kind: 'default',
		alias: 'Umb.ValueSummary.PropertyEditor.ContentPicker',
		name: 'Content Picker Property Editor Value Summary',
		forValueType: UMB_CONTENT_PICKER_PROPERTY_EDITOR_VALUE_TYPE,
		element: () => import('./value-summary.js'),
		valueResolver: () => import('./value-summary.js'),
	},
];
