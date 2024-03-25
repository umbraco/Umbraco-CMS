import type { ManifestTypes } from '@umbraco-cms/backoffice/extension-registry';

const menuItem: ManifestTypes = {
	type: 'menuItem',
	alias: 'Umb.MenuItem.Relations',
	name: 'Relations Menu Item',
	weight: 800,
	meta: {
		label: 'Relations',
		icon: 'icon-trafic',
		entityType: 'relation-type-root',
		menus: ['Umb.Menu.AdvancedSettings'],
	},
};

export const manifests = [menuItem];
