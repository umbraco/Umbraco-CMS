import { UMB_USER_GROUP_REFERENCES_VALUE_TYPE } from './constants.js';

export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'valueSummary',
		kind: 'default',
		alias: 'Umb.ValueSummary.UserGroup.References',
		name: 'User Group References Value Summary',
		forValueType: UMB_USER_GROUP_REFERENCES_VALUE_TYPE,
		element: () => import('./user-group-value-summary.element.js'),
		resolver: () => import('./user-group-value-summary.resolver.js'),
	},
];
