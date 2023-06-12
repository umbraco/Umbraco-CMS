export const name = 'Umbraco.Core.Templating';
export const extensions = [
	{
		name: 'Templating Entry Point',
		alias: 'Umb.EntryPoint.Templating',
		type: 'entryPoint',
		loader: () => import('./package-entry-point.js'),
	},
];
