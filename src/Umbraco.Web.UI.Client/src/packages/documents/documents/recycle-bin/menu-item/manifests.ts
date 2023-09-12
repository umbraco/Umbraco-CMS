import type { ManifestMenuItem } from '@umbraco-cms/backoffice/extension-registry';

const menuItem: ManifestMenuItem = {
	type: 'menuItem',
	alias: 'Umb.MenuItem.DocumentRecycleBin',
	name: 'Document Recycle Bin Menu Item',
	weight: 100,
	loader: () => import('./document-recycle-bin-menu-item.element.js'),
	meta: {
		label: 'Recycle Bin',
		icon: 'umb:trash',
		menus: ['Umb.Menu.Content'],
	},
};

export const manifests = [menuItem];
