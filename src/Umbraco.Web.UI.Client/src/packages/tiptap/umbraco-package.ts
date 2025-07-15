export const name = 'Umbraco.Core.Tiptap';
export const extensions = [
	{
		name: 'Tiptap Bundle',
		alias: 'Umb.Bundle.tiptap',
		type: 'bundle',
		js: () => import('./manifests.js'),
	},
];
