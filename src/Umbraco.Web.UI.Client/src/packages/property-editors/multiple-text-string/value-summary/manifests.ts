import { UMB_MULTIPLE_TEXT_STRING_PROPERTY_EDITOR_VALUE_TYPE } from '../value-type/constants.js';

export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'valueSummary',
		kind: 'default',
		alias: 'Umb.ValueSummary.PropertyEditor.MultipleTextString',
		name: 'Multiple Text String Property Editor Value Summary',
		forValueType: UMB_MULTIPLE_TEXT_STRING_PROPERTY_EDITOR_VALUE_TYPE,
		element: () => import('./value-summary.element.js'),
	},
];
