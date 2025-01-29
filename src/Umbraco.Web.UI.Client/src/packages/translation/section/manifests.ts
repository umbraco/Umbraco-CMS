import { UMB_TRANSLATION_SECTION_ALIAS } from './constants.js';
import { UMB_SECTION_USER_PERMISSION_CONDITION_ALIAS } from '@umbraco-cms/backoffice/section';

export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'section',
		alias: UMB_TRANSLATION_SECTION_ALIAS,
		name: 'Translation Section',
		weight: 400,
		meta: {
			label: '#sections_translation',
			pathname: 'translation',
		},
		conditions: [
			{
				alias: UMB_SECTION_USER_PERMISSION_CONDITION_ALIAS,
				match: UMB_TRANSLATION_SECTION_ALIAS,
			},
		],
	},
];
