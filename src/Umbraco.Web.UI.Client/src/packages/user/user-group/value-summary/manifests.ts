import { UMB_USER_GROUP_REFERENCES_VALUE_TYPE } from './constants.js';
import { manifests as sectionManifests } from './sections/manifests.js';

export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'valueSummary',
		alias: 'Umb.ValueSummary.UserGroup.References',
		name: 'User Group References Value Summary',
		forValueType: UMB_USER_GROUP_REFERENCES_VALUE_TYPE,
		element: () => import('./user-group-value-summary.element.js'),
		api: () => import('./user-group-value-summary.api.js'),
	},
	...sectionManifests,
];
