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
		type: 'entryPoint',
		js: () => import('./entry-point.js'),
	},
];
