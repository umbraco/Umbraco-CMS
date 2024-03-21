import type { ManifestTypes } from '@umbraco-cms/backoffice/extension-registry';

const menuItem: ManifestTypes = {
	type: 'menuItem',
	alias: 'Umb.MenuItem.RelationTypes',
	name: 'Relation Types Menu Item',
	weight: 500,
	meta: {
		label: 'Relation Types',
		icon: 'icon-trafic',
		entityType: 'relation-type-root',
		menus: ['Umb.Menu.Settings'],
	},
};

export const manifests = [menuItem];
