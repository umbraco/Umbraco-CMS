import { UMB_ELEMENT_PICKER_PROPERTY_EDITOR_VALUE_TYPE } from '../value-type/constants.js';

export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'valueSummary',
		kind: 'default',
		alias: 'Umb.ValueSummary.PropertyEditor.ElementPicker',
		name: 'Element Picker Property Editor Value Summary',
		forValueType: UMB_ELEMENT_PICKER_PROPERTY_EDITOR_VALUE_TYPE,
		element: () => import('./value-summary.element.js'),
		valueResolver: () => import('./value-summary.resolver.js'),
	},
];
