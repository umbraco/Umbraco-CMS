export const name = 'Umbraco.Core.UmbracoNews';
export const extensions = [
	{
		name: 'Umbraco News Bundle',
		alias: 'Umb.Bundle.UmbracoNews',
		type: 'bundle',
		js: () => import('./manifests.js'),
	},
];
