import { UMB_TIME_ONLY_PROPERTY_EDITOR_VALUE_TYPE } from '../value-type/constants.js';

export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'valueSummary',
		kind: 'default',
		alias: 'Umb.ValueSummary.PropertyEditor.TimeOnly',
		name: 'Time Only Property Editor Value Summary',
		forValueType: UMB_TIME_ONLY_PROPERTY_EDITOR_VALUE_TYPE,
		element: () => import('../../date-time-value-summary.element.js'),
	},
];
