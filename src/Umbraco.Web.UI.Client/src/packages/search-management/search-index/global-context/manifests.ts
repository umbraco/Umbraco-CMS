export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'globalContext',
		alias: 'Umbraco.Search.GlobalContext',
		name: 'Umbraco Search Global Context',
		api: () => import('./search.global-context.js'),
	},
];
