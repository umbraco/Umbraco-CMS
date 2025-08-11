export const name = 'Umbraco.ManagementApi';
export const extensions = [
	{
		name: 'Management Api Bundle',
		alias: 'Umb.Bundle.ManagementApi',
		type: 'bundle',
		js: () => import('./manifests.js'),
	},
];
