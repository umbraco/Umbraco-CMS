export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'valueMinimalDisplay',
		alias: 'Umb.ValueMinimalDisplay.User.State',
		name: 'User State Value Minimal Display',
		element: () => import('./state/user-state-value-minimal-display.element.js'),
	},
	{
		type: 'valueMinimalDisplay',
		alias: 'Umb.ValueMinimalDisplay.User.LastLogin',
		name: 'User Last Login Value Minimal Display',
		element: () => import('./last-login/user-last-login-value-minimal-display.element.js'),
	},
];
