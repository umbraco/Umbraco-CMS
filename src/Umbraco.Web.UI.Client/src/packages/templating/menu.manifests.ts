import type { ManifestMenu, ManifestTypes } from '@umbraco-cms/backoffice/extension-registry';

const sectionAlias = 'Umb.Section.Settings';

const menu: ManifestMenu = {
	type: 'menu',
	alias: 'Umb.Menu.Templating',
	name: 'Templating Menu',
};

const menuSectionSidebarApp: ManifestTypes = {
	type: 'sectionSidebarApp',
	kind: 'menu',
	alias: 'Umb.SectionSidebarMenu.Templating',
	name: 'Templating Section Sidebar Menu',
	weight: 200,
	meta: {
		label: 'Templating',
		menu: 'Umb.Menu.Templating',
	},
	conditions: [
		{
			alias: 'Umb.Condition.SectionAlias',
			match: sectionAlias,
		},
	],
};

export const manifests: Array<ManifestTypes> = [menu, menuSectionSidebarApp];
