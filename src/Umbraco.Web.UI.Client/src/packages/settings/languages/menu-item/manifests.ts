import type { ManifestMenuItem } from '@umbraco-cms/backoffice/extension-registry';

const menuItem: ManifestMenuItem = {
	type: 'menuItem',
	alias: 'Umb.MenuItem.Languages',
	name: 'Languages Menu Item',
	weight: 100,
	meta: {
		label: 'Languages',
		icon: 'umb:globe',
		entityType: 'language-root',
		menus: ['Umb.Menu.Settings'],
	},
};

export const manifests = [menuItem];
