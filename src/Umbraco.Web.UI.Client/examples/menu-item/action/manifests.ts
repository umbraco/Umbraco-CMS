import { UMB_CONTENT_MENU_ALIAS } from '@umbraco-cms/backoffice/document';

export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'menuItem',
		kind: 'action',
		alias: 'Example.MenuItem.Action',
		name: 'Example Action Menu Item',
		api: () => import('./action-menu-item.api.js'),
		meta: {
			label: 'Example Action Menu Item',
			icon: 'icon-hammer',
			menus: [UMB_CONTENT_MENU_ALIAS],
		},
	},
];
