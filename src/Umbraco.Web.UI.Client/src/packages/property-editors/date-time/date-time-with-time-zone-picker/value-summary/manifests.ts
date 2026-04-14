import { UMB_DATE_TIME_WITH_TIME_ZONE_PROPERTY_EDITOR_SCHEMA_VALUE_TYPE } from '../value-type/constants.js';

export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'valueSummary',
		kind: 'default',
		alias: 'Umb.ValueSummary.PropertyEditorSchema.DateTimeWithTimeZone',
		name: 'Date Time With Time Zone Property Editor Schema Value Summary',
		forValueType: UMB_DATE_TIME_WITH_TIME_ZONE_PROPERTY_EDITOR_SCHEMA_VALUE_TYPE,
		element: () => import('./value-summary.element.js'),
	},
];
