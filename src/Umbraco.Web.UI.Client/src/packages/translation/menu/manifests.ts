import { UMB_TRANSLATION_SECTION_ALIAS } from '../section/index.js';
import { UMB_TRANSLATION_MENU_ALIAS } from './constants.js';

export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'menu',
		alias: UMB_TRANSLATION_MENU_ALIAS,
		name: 'Translation Menu',
	},
	{
		type: 'sectionSidebarApp',
		kind: 'menuWithEntityActions',
		alias: 'Umb.SidebarMenu.Translation',
		name: 'Translation Sidebar Menu',
		weight: 100,
		meta: {
			label: '#general_dictionary', // We are using dictionary here on purpose until dictionary has its own menu item.
			menu: UMB_TRANSLATION_MENU_ALIAS,
			entityType: 'dictionary-root', // hard-coded on purpose to avoid circular dependency. We need another way to add actions to a menu kind.
		},
		conditions: [
			{
				alias: 'Umb.Condition.SectionAlias',
				match: UMB_TRANSLATION_SECTION_ALIAS,
			},
		],
	},
];
