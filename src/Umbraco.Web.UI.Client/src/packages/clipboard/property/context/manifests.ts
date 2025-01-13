export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'propertyContext',
		name: 'Clipboard Property Context',
		alias: 'Umb.PropertyContext.Clipboard',
		api: () => import('./clipboard.property-context.js'),
	},
];
