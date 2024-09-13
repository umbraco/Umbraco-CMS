export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'kind',
		alias: 'Umb.Kind.MenuItem.Link',
		matchKind: 'link',
		matchType: 'menuItem',
		manifest: {
			type: 'menuItem',
			element: () => import('./link-menu-item.element.js'),
		},
	},
];
