export const name = 'Umbraco.Sorter';
export const extensions = [
	{
		name: 'Sorter Bundle',
		alias: 'Umb.Bundle.Sorter',
		type: 'bundle',
		js: () => import('./manifests.js'),
	},
];
