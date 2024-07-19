import type { ManifestMenuItem, ManifestTypes } from '@umbraco-cms/backoffice/extension-registry';

const menuItem: ManifestMenuItem = {
	type: 'menuItem',
	alias: 'Umb.MenuItem.LogViewer',
	name: 'Log Viewer Menu Item',
	weight: 300,
	meta: {
		label: '#treeHeaders_logViewer',
		icon: 'icon-box-alt',
		entityType: 'logviewer',
		menus: ['Umb.Menu.AdvancedSettings'],
	},
};

export const manifests: Array<ManifestTypes> = [menuItem];
