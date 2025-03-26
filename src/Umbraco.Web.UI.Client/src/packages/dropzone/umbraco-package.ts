export const name = 'Umbraco.Core.Dropzone';
export const extensions = [
	{
		name: 'Dropzone Entry Point',
		alias: 'Umb.EntryPoint.Dropzone',
		type: 'backofficeEntryPoint',
		js: () => import('./entry-point.js'),
	},
];
