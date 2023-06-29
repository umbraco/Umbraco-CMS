export const name = 'Umbraco.Core.MediaManagement';
export const extensions = [
	{
		name: 'Media Management Bundle',
		alias: 'Umb.Bundle.MediaManagement',
		type: 'bundle',
		loader: () => import('./manifests.js'),
	},
];
