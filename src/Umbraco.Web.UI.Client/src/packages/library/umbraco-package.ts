export const name = 'Umbraco.Core.Library';
export const extensions = [
	{
		name: 'Umbraco Library Bundle',
		alias: 'Umb.Bundle.Library',
		type: 'bundle',
		js: () => import('./manifests.js'),
	},
];
