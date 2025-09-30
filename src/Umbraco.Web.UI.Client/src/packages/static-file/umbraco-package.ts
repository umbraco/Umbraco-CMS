export const name = 'Umbraco.Core.StaticFileManagement';
export const extensions = [
	{
		name: 'Static File Management Bundle',
		alias: 'Umb.Bundle.StaticFileManagement',
		type: 'bundle',
		js: () => import('./manifests.js'),
	},
];
