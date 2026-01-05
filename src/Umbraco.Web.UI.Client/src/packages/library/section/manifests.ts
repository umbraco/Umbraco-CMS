import { UMB_LIBRARY_SECTION_ALIAS } from './constants.js';
import { UMB_SECTION_USER_PERMISSION_CONDITION_ALIAS } from '@umbraco-cms/backoffice/section';

export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'section',
		alias: UMB_LIBRARY_SECTION_ALIAS,
		name: 'Library Section',
		weight: 850,
		meta: {
			label: '#sections_library',
			pathname: 'library',
		},
		conditions: [
			{
				alias: UMB_SECTION_USER_PERMISSION_CONDITION_ALIAS,
				match: UMB_LIBRARY_SECTION_ALIAS,
			},
		],
	},
];
