export const name = 'Umbraco.Core.DynamicRoot';
export const extensions = [
	{
		name: 'Dynamic Root Bundle',
		alias: 'Umb.Bundle.DynamicRoot',
		type: 'bundle',
		js: () => import('./manifests.js'),
	},
];
