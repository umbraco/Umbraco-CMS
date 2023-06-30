export const name = 'Umbraco.Core.Templating';
export const extensions = [
	{
		name: 'Template Management Bundle',
		alias: 'Umb.Bundle.TemplateManagement',
		type: 'bundle',
		loader: () => import('./manifests.js'),
	},
];
