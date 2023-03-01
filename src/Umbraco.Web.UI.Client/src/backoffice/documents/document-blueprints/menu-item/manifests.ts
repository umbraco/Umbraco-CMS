import type { ManifestMenuItem } from '@umbraco-cms/models';

const menuItem: ManifestMenuItem = {
	type: 'menuItem',
	alias: 'Umb.MenuItem.DocumentBlueprints',
	name: 'Document Blueprints Menu Item',
	weight: 90,
	meta: {
		label: 'Document Blueprints',
		icon: 'umb:blueprint',
		menus: ['Umb.Menu.Settings'],
		entityType: 'document-blueprint-root',
	},
};

export const manifests = [menuItem];
