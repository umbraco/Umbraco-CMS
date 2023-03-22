import type { ManifestMenuItem } from '@umbraco-cms/backoffice/extensions-registry';

const menuItem: ManifestMenuItem = {
	type: 'menuItem',
	alias: 'Umb.MenuItem.Languages',
	name: 'Languages Menu Item',
	weight: 80,
	meta: {
		label: 'Languages',
		icon: 'umb:globe',
		entityType: 'language-root',
	},
	conditions: {
		menus: ['Umb.Menu.Settings'],
	},
};

export const manifests = [menuItem];
