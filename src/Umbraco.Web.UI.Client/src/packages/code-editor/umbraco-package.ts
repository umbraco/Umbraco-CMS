export const extensions = [
	{
		name: 'Umbraco Code Editor Bundle',
		alias: 'Umb.Bundle.UmbracoCodeEditor',
		type: 'bundle',
		js: () => import('./manifests.js'),
	},
];
