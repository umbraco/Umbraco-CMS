export const extensions = [
	{
		name: 'Umbraco Flavored Markdown Bundle',
		alias: 'Umb.Bundle.UmbracoFlavoredMarkdown',
		type: 'bundle',
		js: () => import('./manifests.js'),
	},
];
