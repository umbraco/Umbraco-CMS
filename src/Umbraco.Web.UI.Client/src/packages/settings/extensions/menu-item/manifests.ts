import type { ManifestMenuItem } from '@umbraco-cms/backoffice/extension-registry';

const menuItem: ManifestMenuItem = {
	type: 'menuItem',
	alias: 'Umb.MenuItem.Extensions',
	name: 'Extensions Menu Item',
	weight: 0,
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
