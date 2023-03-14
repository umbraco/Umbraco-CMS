import type { ManifestMenuItem } from '@umbraco-cms/models';

const menuItem: ManifestMenuItem = {
	type: 'menuItem',
	alias: 'Umb.MenuItem.LogViewer',
	name: 'LogViewer Menu Item',
	weight: 70,
	meta: {
		label: 'Log Viewer',
		icon: 'umb:box-alt',
		entityType: 'logviewer',
		menus: ['Umb.Menu.Settings'],
	},
};

export const manifests = [menuItem];
