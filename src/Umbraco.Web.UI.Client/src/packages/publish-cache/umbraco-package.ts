export const name = 'Umbraco.Core.PublishCache';
export const extensions = [
	{
		name: 'Publish Cache Bundle',
		alias: 'Umb.Bundle.PublishCache',
		type: 'bundle',
		js: () => import('./manifests.js'),
	},
];
