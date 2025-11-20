export const name = 'Umbraco.Core.Preview';
export const extensions = [
	{
		name: 'Preview Bundle',
		alias: 'Umb.Bundle.Preview',
		type: 'bundle',
		js: () => import('./manifests.js'),
	},
];
