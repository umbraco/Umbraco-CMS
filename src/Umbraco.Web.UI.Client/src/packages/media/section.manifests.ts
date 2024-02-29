import { UMB_MEDIA_ROOT_ENTITY_TYPE, UMB_MEDIA_MENU_ALIAS } from './media/index.js';
import type { ManifestSection, ManifestSectionSidebarApp } from '@umbraco-cms/backoffice/extension-registry';

const sectionAlias = 'Umb.Section.Media';

const section: ManifestSection = {
	type: 'section',
	alias: sectionAlias,
	name: 'Media Section',
	weight: 500,
	meta: {
		label: 'Media',
		pathname: 'media-management',
	},
	conditions: [],
};

const menuSectionSidebarApp: ManifestSectionSidebarApp = {
	type: 'sectionSidebarApp',
	kind: 'menuWithEntityActions',
	alias: 'Umb.SectionSidebarMenu.Media',
	name: 'Media Section Sidebar Menu',
	weight: 100,
	meta: {
		label: 'Media',
		menu: UMB_MEDIA_MENU_ALIAS,
		entityType: UMB_MEDIA_ROOT_ENTITY_TYPE,
	},
	conditions: [
		{
			alias: 'Umb.Condition.SectionAlias',
			match: sectionAlias,
		},
	],
};

export const manifests = [section, menuSectionSidebarApp];
