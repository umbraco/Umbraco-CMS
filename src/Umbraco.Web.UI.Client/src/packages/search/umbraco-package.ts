export const name = 'Umbraco.Core.Search';
export const extensions = [
	{
		name: 'Search Bundle',
		alias: 'Umb.Bundle.Search',
		type: 'bundle',
		js: () => import('./manifests.js'),
	},
];
