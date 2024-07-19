import type { ManifestMenuItem, ManifestTypes } from '@umbraco-cms/backoffice/extension-registry';

const menuItem: ManifestMenuItem = {
	type: 'menuItem',
	alias: 'Umb.MenuItem.Extensions',
	name: 'Extension Insights Menu Item',
	weight: 200,
	meta: {
		label: 'Extension Insights',
		icon: 'icon-wand',
		entityType: 'extension-root',
		menus: ['Umb.Menu.AdvancedSettings'],
	},
};

export const manifests: Array<ManifestTypes> = [menuItem];
