import { UMB_DATE_TIME_VALUE_SUMMARY_ALIAS } from './constants.js';
import { UMB_DATE_TIME_VALUE_TYPE } from '@umbraco-cms/backoffice/value-type';

export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'valueSummary',
		alias: UMB_DATE_TIME_VALUE_SUMMARY_ALIAS,
		name: 'Date Time Value Summary',
		forValueType: UMB_DATE_TIME_VALUE_TYPE,
		element: () => import('./date-time-value-summary.element.js'),
	},
];
