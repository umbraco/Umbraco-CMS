export const name = 'Umbraco.Core.MarkdownEditor';
export const extensions = [
	{
		name: 'Markdown Editor Bundle',
		alias: 'Umb.Bundle.MarkdownEditor',
		type: 'bundle',
		js: () => import('./manifests.js'),
	},
];
