export const name = 'Umbraco.Sort';
export const extensions = [
	{
		name: 'Sort Bundle',
		alias: 'Umb.Bundle.Sort',
		type: 'bundle',
		js: () => import('./manifests.js'),
	},
];
