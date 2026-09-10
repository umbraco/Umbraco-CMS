export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'condition',
		name: 'Search Index Provider Name Condition',
		alias: 'Umb.Search.Condition.IndexProviderName',
		api: () => import('./indexProviderName.condition.js'),
	},
];
