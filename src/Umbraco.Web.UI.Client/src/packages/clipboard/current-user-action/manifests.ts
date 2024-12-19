export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'currentUserAction',
		kind: 'default',
		alias: 'Umb.CurrentUserAction.Clipboard',
		name: 'Current User Clipboard Button',
		weight: 100,
		api: () => import('./clipboard.current-user-action.js'),
		meta: {
			label: 'Clipboard',
		},
	},
];
