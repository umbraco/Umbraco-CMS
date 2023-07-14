import { ManifestMenu, ManifestTypes } from '@umbraco-cms/backoffice/extension-registry';

const sectionAlias = 'Umb.Section.Settings';

const menu: ManifestMenu = {
	type: 'menu',
	alias: 'Umb.Menu.Templating',
	name: 'Templating Menu',
	meta: {
		label: 'Templating',
	},
};

const menuSectionSidebarApp: ManifestTypes = {
	type: 'sectionSidebarApp',
	kind: 'menu',
	alias: 'Umb.SectionSidebarMenu.Templating',
	name: 'Templating Section Sidebar Menu',
	weight: 100,
	meta: {
		label: 'Templating',
		menu: 'Umb.Menu.Templating',
	},
	conditions: [
		{
			alias: 'Umb.Condition.SectionAlias',
			value: sectionAlias,
		},
	],
};

export const manifests = [menu, menuSectionSidebarApp];
