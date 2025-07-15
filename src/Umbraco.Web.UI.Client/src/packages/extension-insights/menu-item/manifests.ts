export const manifests: Array<UmbExtensionManifest> = [
	{
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
	},
];
