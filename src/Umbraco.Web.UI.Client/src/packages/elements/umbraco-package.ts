export const name = 'Umbraco.Core.ElementManagement';
export const extensions = [
	{
		name: 'Umbraco Element Management Bundle',
		alias: 'Umb.Bundle.ElementManagement',
		type: 'bundle',
		js: () => import('./manifests.js'),
	},
];
