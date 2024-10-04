export const manifests: Array<UmbExtensionManifest> = [
	{
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
	},
];
