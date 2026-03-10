import * as entryPointModule from './entry-point.js';

export const name = 'Umbraco.Core.PropertyEditors';
export const extensions = [
	{
		name: 'Property Editors Bundle',
		alias: 'Umb.Bundle.PropertyEditors',
		type: 'bundle',
		js: () => import('./manifests.js'),
	},
	{
		name: 'Property Editors Entry Point',
		alias: 'Umb.EntryPoint.PropertyEditors',
		type: 'backofficeEntryPoint',
		js: entryPointModule,
	},
];
