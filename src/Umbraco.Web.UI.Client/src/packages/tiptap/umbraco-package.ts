export const name = 'Umbraco.Core.Tiptap';
export const extensions = [
	{
		name: 'Tiptap Bundle',
		alias: 'Umb.Bundle.Tiptap',
		type: 'bundle',
		js: () => import('./manifests.js'),
	},
];
