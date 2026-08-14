import { UMB_DATE_TIME_UNSPECIFIED_PROPERTY_EDITOR_VALUE_TYPE } from '../value-type/constants.js';

export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'valueSummary',
		kind: 'default',
		alias: 'Umb.ValueSummary.PropertyEditor.DateTimeUnspecified',
		name: 'Date Time Unspecified Property Editor Value Summary',
		forValueType: UMB_DATE_TIME_UNSPECIFIED_PROPERTY_EDITOR_VALUE_TYPE,
		element: () => import('../../date-time-value-summary.element.js'),
	},
];
