export const name = 'Umbraco.Core.MultiUrlPicker';
export const extensions = [
	{
		name: 'Multi Url Picker Bundle',
		alias: 'Umb.Bundle.MultiUrlPicker',
		type: 'bundle',
		js: () => import('./manifests.js'),
	},
	{
		name: 'Multi Url Picker Entry Point',
		alias: 'Umb.EntryPoint.MultiUrlPicker',
		type: 'entryPoint',
		js: () => import('./entry-point.js'),
	},
];
