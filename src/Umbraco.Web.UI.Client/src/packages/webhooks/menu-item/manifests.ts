import type { ManifestMenuItem } from '@umbraco-cms/backoffice/extension-registry';

const menuItem: ManifestMenuItem = {
	type: 'menuItem',
	alias: 'Umb.MenuItem.Webhooks',
	name: 'Webhooks Menu Item',
	weight: 100,
	meta: {
		label: 'Webhooks',
		icon: 'icon-webhook',
		entityType: 'webhooks',
		menus: ['Umb.Menu.Settings'],
	},
};

export const manifests = [menuItem];
