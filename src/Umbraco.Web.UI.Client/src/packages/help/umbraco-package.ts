export const name = 'Umbraco.Core.Help';
export const extensions = [
	{
		name: 'Help Bundle',
		alias: 'Umb.Bundle.Help',
		type: 'bundle',
		js: () => import('./manifests.js'),
	},
];
