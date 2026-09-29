import { UMB_CHECKBOX_LIST_PROPERTY_EDITOR_VALUE_TYPE } from '../value-type/constants.js';

export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'valueSummary',
		kind: 'default',
		alias: 'Umb.ValueSummary.PropertyEditor.CheckboxList',
		name: 'Checkbox List Property Editor Value Summary',
		forValueType: UMB_CHECKBOX_LIST_PROPERTY_EDITOR_VALUE_TYPE,
		element: () => import('./value-summary.element.js'),
	},
];
