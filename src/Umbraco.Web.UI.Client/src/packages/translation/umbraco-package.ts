export const name = 'Umbraco.Core.Translation';
export const extensions = [
	{
		name: 'Umbraco Translation Bundle',
		alias: 'Umb.Bundle.Translation',
		type: 'bundle',
		js: () => import('./manifests.js'),
	},
];
