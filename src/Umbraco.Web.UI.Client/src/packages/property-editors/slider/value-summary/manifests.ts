import {
	UMB_RANGE_SLIDER_PROPERTY_EDITOR_VALUE_TYPE,
	UMB_SLIDER_PROPERTY_EDITOR_VALUE_TYPE,
} from '../value-type/constants.js';

export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'valueSummary',
		kind: 'default',
		alias: 'Umb.ValueSummary.PropertyEditor.Slider',
		name: 'Slider Property Editor Value Summary',
		forValueType: UMB_SLIDER_PROPERTY_EDITOR_VALUE_TYPE,
		element: () => import('./value-summary.element.js'),
	},
	{
		type: 'valueSummary',
		kind: 'default',
		alias: 'Umb.ValueSummary.PropertyEditor.RangeSlider',
		name: 'Range Slider Property Editor Value Summary',
		forValueType: UMB_RANGE_SLIDER_PROPERTY_EDITOR_VALUE_TYPE,
		element: () => import('./value-summary.element.js'),
	},
];
