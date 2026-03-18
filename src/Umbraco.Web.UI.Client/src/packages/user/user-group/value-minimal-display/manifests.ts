export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'valueMinimalDisplay',
		alias: 'Umb.ValueMinimalDisplay.User.UserGroups',
		name: 'User Groups Value Minimal Display',
		element: () => import('./user-group-value-minimal-display.element.js'),
		api: () => import('./user-group-value-minimal-display.api.js'),
		meta: { label: 'User Groups' },
	},
];
