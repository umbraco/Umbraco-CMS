export const name = 'Umbraco.Core.Relations';
export const extensions = [
	{
		name: 'Relations Bundle',
		alias: 'Umb.Bundle.Relations',
		type: 'bundle',
		js: () => import('./manifests.js'),
	},
];
