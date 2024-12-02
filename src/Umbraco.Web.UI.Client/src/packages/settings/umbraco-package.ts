export const name = 'Umbraco.Core.Settings';
export const extensions = [
	{
		name: 'Settings Bundle',
		alias: 'Umb.Bundle.Settings',
		type: 'bundle',
		js: () => import('./manifests.js'),
	},
];
