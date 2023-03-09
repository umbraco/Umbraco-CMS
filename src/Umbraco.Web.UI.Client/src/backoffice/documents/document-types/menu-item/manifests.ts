import type { ManifestMenuItem } from '@umbraco-cms/models';

const menuItem: ManifestMenuItem = {
	type: 'menuItem',
	alias: 'Umb.MenuItem.DocumentTypes',
	name: 'Document Types Menu Item',
	weight: 10,
	loader: () => import('./document-types-menu-item.element'),
	meta: {
		label: 'Document Types',
		icon: 'umb:folder',
		menus: ['Umb.Menu.Settings'],
	},
};

export const manifests = [menuItem];
