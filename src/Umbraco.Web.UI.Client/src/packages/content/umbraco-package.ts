export const name = 'Umbraco.Content';
export const extensions = [
	{
		name: 'Content Bundle',
		alias: 'Umb.Bundle.Content',
		type: 'bundle',
		js: () => import('./manifests.js'),
	},
];
