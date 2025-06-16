import { UMB_MEMBER_MANAGEMENT_SECTION_ALIAS } from '../section/index.js';
import { UMB_MEMBER_MANAGEMENT_DASHBOARD_ALIAS } from './constants.js';

export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'dashboard',
		kind: 'default',
		alias: UMB_MEMBER_MANAGEMENT_DASHBOARD_ALIAS,
		name: 'Member Management Overview Dashboard',
		weight: 1000,
		meta: {
			label: 'Overview',
			pathname: 'overview',
		},
		conditions: [
			{
				alias: 'Umb.Condition.SectionAlias',
				match: UMB_MEMBER_MANAGEMENT_SECTION_ALIAS,
			},
		],
	},
];
