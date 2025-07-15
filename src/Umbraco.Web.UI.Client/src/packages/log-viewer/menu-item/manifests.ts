export const manifests: Array<UmbExtensionManifest> = [
	{
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
	},
];
