export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'globalContext',
		alias: 'Umb.GlobalContext.Clipboard',
		name: 'Clipboard Context',
		api: () => import('./clipboard.context.js'),
	},
];
