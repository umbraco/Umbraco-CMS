import type { ManifestMenuItem, ManifestTypes } from '@umbraco-cms/backoffice/extension-registry';

const menuItem: ManifestMenuItem = {
	type: 'menuItem',
	alias: 'Umb.MenuItem.Webhook',
	name: 'Webhook Menu Item',
	weight: 100,
	meta: {
		label: '#treeHeaders_webhooks',
		icon: 'icon-webhook',
		entityType: 'webhook-root',
		menus: ['Umb.Menu.AdvancedSettings'],
	},
};

export const manifests: Array<ManifestTypes> = [menuItem];
