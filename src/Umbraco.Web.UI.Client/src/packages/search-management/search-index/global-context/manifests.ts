export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'globalContext',
		alias: 'Umb.Search.GlobalContext',
		name: 'Umbraco Search Global Context',
		api: () => import('./search.global-context.js'),
	},
];
