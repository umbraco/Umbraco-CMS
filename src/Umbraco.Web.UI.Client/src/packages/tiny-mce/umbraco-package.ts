export const name = 'Umbraco.Core.TinyMce';
export const extensions = [
	{
		name: 'TinyMce Bundle',
		alias: 'Umb.Bundle.TinyMce',
		type: 'bundle',
		js: () => import('./manifests.js'),
	},
];
