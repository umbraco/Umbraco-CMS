export const name = 'Umbraco.Core.Search';
export const extensions = [
	{
		name: 'Search Bundle',
		alias: 'Umb.Bundle.Search',
		type: 'bundle',
		loader: () => import('./manifests.js'),
	},
];
