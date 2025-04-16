export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'menuItem',
		alias: 'Umb.MenuItem.Relations',
		name: 'Relations Menu Item',
		weight: 800,
		meta: {
			label: '#treeHeaders_relations',
			icon: 'icon-trafic',
			entityType: 'relations-root',
			menus: ['Umb.Menu.AdvancedSettings'],
		},
	},
];
