import type { ManifestMenuItem } from '@umbraco-cms/models';

const menuItem: ManifestMenuItem = {
	type: 'menuItem',
	alias: 'Umb.MenuItem.Languages',
	name: 'Languages Menu Item',
	weight: 80,
	meta: {
		label: 'Languages',
		icon: 'umb:globe',
		entityType: 'language-root',
		menus: ['Umb.Menu.Settings'],
	},
};

export const manifests = [menuItem];
