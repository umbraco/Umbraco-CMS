export const name = 'Umbraco.Core.Templating';
export const extensions = [
	{
		name: 'Template Management Bundle',
		alias: 'Umb.Bundle.TemplateManagement',
		type: 'bundle',
		js: () => import('./manifests.js'),
	},
];
