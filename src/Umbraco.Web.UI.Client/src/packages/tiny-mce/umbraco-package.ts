export const name = 'Umbraco.Core.Rte';
export const extensions = [
	{
		name: 'TinyMCE Bundle',
		alias: 'Umb.Bundle.TinyMCE',
		type: 'bundle',
		js: () => import('./manifests.js'),
	},
];
