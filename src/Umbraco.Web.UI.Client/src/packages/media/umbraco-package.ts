export const name = 'Umbraco.Core.MediaManagement';
export const extensions = [
	{
		name: 'Media Management Bundle',
		alias: 'Umb.Bundle.MediaManagement',
		type: 'bundle',
		js: () => import('./manifests.js'),
	},
	{
		name: 'Media Management Entry Point',
		alias: 'Umb.EntryPoint.MediaManagement',
		type: 'backofficeEntryPoint',
		js: () => import('./entry-point.js'),
	},
];
