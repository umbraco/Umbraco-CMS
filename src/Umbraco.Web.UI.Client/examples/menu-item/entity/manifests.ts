import { UMB_CONTENT_MENU_ALIAS } from '@umbraco-cms/backoffice/document';

export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'menuItem',
		alias: 'Example.MenuItem.Entity',
		name: 'Example Entity Menu Item',
		meta: {
			label: 'Example Entity Menu Item',
			icon: 'icon-wand',
			entityType: 'example-entity-type',
			menus: [UMB_CONTENT_MENU_ALIAS],
		},
	},
];
