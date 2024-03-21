import type { ManifestTypes } from '@umbraco-cms/backoffice/extension-registry';

const menuItem: ManifestTypes = {
	type: 'menuItem',
	alias: 'Umb.MenuItem.Relations',
	name: 'Relations Menu Item',
	weight: 500,
	meta: {
		label: 'Relations',
		icon: 'icon-trafic',
		entityType: 'relation-type-root',
		menus: ['Umb.Menu.Settings'],
	},
};

export const manifests = [menuItem];
