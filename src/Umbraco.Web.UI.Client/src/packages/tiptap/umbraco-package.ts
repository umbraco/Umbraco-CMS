export const name = 'Umbraco.Core.Rte';
export const extensions = [
	{
		name: 'Tip Tap Bundle',
		alias: 'Umb.Bundle.tiptap',
		type: 'bundle',
		js: () => import('./manifests.js'),
	},
];
