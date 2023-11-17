export const name = 'Umbraco.Core.LogViewer';
export const extensions = [
	{
		name: 'Log Viewer Bundle',
		alias: 'Umb.Bundle.LogViewer',
		type: 'bundle',
		js: () => import('./manifests.js'),
	},
];
