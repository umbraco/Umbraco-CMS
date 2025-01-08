import { UMB_TRANSLATION_SECTION_ALIAS } from './constants.js';

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
				alias: 'Umb.Condition.SectionUserPermission',
				match: UMB_TRANSLATION_SECTION_ALIAS,
			},
		],
	},
];
