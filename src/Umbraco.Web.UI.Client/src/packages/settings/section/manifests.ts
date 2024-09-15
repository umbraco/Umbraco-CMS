import { UMB_SETTINGS_SECTION_ALIAS } from './constants.js';
import type { UmbExtensionManifestKind } from '@umbraco-cms/backoffice/extension-registry';

export const manifests: Array<UmbExtensionManifest | UmbExtensionManifestKind> = [
	{
		type: 'section',
		alias: UMB_SETTINGS_SECTION_ALIAS,
		name: 'Settings Section',
		weight: 800,
		meta: {
			label: '#sections_settings',
			pathname: 'settings',
		},
		conditions: [
			{
				alias: 'Umb.Condition.SectionUserPermission',
				match: UMB_SETTINGS_SECTION_ALIAS,
			},
		],
	},
];
