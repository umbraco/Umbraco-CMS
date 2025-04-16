export const name = 'Umbraco.Clipboard';
export const extensions = [
	{
		name: 'Clipboard Bundle',
		alias: 'Umb.Bundle.Clipboard',
		type: 'bundle',
		js: () => import('./manifests.js'),
	},
];
