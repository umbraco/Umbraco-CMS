export const name = 'Umbraco.Core.EmbeddedMedia';
export const extensions = [
	{
		name: 'Embedded Media Bundle',
		alias: 'Umb.Bundle.EmbeddedMedia',
		type: 'bundle',
		js: () => import('./manifests.js'),
	},
];
