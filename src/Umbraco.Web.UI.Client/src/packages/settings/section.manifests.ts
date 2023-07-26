import { ManifestSectionSidebarAppMenuKind } from '@umbraco-cms/backoffice/extension-registry';
import type { ManifestSection } from '@umbraco-cms/backoffice/extension-registry';

const sectionAlias = 'Umb.Section.Settings';

const section: ManifestSection = {
	type: 'section',
	alias: sectionAlias,
	name: 'Settings Section',
	weight: 400,
	meta: {
		label: 'Settings',
		pathname: 'settings',
	},
};

const menuSectionSidebarApp: ManifestSectionSidebarAppMenuKind = {
	type: 'sectionSidebarApp',
	kind: 'menu',
	alias: 'Umb.SectionSidebarMenu.Settings',
	name: 'Settings Section Sidebar Menu',
	weight: 200,
	meta: {
		label: 'Settings',
		menu: 'Umb.Menu.Settings',
	},
	conditions: [
		{
			alias: 'Umb.Condition.SectionAlias',
			match: sectionAlias,
		},
	],
};

export const manifests = [section, menuSectionSidebarApp];
