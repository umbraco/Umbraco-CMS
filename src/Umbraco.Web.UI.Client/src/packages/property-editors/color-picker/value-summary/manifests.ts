import { UMB_COLOR_PICKER_PROPERTY_EDITOR_VALUE_TYPE } from '../value-type/constants.js';

export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'valueSummary',
		kind: 'default',
		alias: 'Umb.ValueSummary.PropertyEditor.ColorPicker',
		name: 'Color Picker Property Editor Value Summary',
		forValueType: UMB_COLOR_PICKER_PROPERTY_EDITOR_VALUE_TYPE,
		element: () => import('./value-summary.element.js'),
	},
];
