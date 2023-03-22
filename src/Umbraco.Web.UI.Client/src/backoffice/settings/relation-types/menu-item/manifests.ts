import type { ManifestMenuItem } from '@umbraco-cms/backoffice/extensions-registry';

const menuItem: ManifestMenuItem = {
	type: 'menuItem',
	alias: 'Umb.MenuItem.RelationTypes',
	name: 'Relation Types Menu Item',
	weight: 40,
	loader: () => import('./relation-types-menu-item.element'),
	meta: {
		label: 'Relation Types',
		icon: 'umb:folder',
		entityType: 'relation-type',
	},
	conditions: {
		menus: ['Umb.Menu.Settings'],
	},
};

export const manifests = [menuItem];
