import { UMB_MEMBER_GROUP_UNIQUES_VALUE_TYPE } from '../value-type/constants.js';

export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'valueSummary',
		kind: 'default',
		alias: 'Umb.ValueSummary.MemberGroup.Uniques',
		name: 'Member Group Uniques Value Summary',
		forValueType: UMB_MEMBER_GROUP_UNIQUES_VALUE_TYPE,
		element: () => import('./member-group-uniques-value-summary.js'),
		valueResolver: () => import('./member-group-uniques-value-summary.js'),
	},
];
