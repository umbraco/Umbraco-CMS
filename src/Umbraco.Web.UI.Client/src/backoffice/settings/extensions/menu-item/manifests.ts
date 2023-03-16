import type { ManifestMenuItem } from '@umbraco-cms/models';

const menuItem: ManifestMenuItem = {
	type: 'menuItem',
	alias: 'Umb.MenuItem.Extensions',
	name: 'Extensions Menu Item',
	weight: 100,
	meta: {
		label: 'Extensions',
		icon: 'umb:wand',
		entityType: 'extension-root',
	},
	conditions: {
		menus: ['Umb.Menu.Settings'],
	},
};

export const manifests = [menuItem];
