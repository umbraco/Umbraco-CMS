import { UMB_CONTENT_MENU_ALIAS } from '@umbraco-cms/backoffice/document';

export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'menuItem',
		kind: 'link',
		alias: 'Example.MenuItem.ExternalLink',
		name: 'Example External Link Menu Item',
		meta: {
			label: 'Example Link Menu Item',
			icon: 'icon-link',
			href: 'https://',
			menus: [UMB_CONTENT_MENU_ALIAS],
		},
	},
];
