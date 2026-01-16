import * as entryPointModule from './entry-point.js';

export const name = 'Umbraco.Core.Templating';
export const extensions = [
	{
		name: 'Template Management Bundle',
		alias: 'Umb.Bundle.TemplateManagement',
		type: 'bundle',
		js: () => import('./manifests.js'),
	},
	{
		name: 'Template Management Backoffice Entry Point',
		alias: 'Umb.BackofficeEntryPoint.TemplateManagement',
		type: 'backofficeEntryPoint',
		js: entryPointModule,
	},
];
