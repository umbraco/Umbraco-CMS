export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'propertyContext',
		name: 'Clipboard Property Context',
		alias: 'Umb.PropertyContext.Clipboard',
		api: () => import('./property-clipboard.context.js'),
	},
];
