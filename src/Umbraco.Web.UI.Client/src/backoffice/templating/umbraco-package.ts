export const name = 'Umbraco.Core.Templating';
export const version = '0.0.1';
export const extensions = [
	{
		name: 'Templating Entry Point',
		alias: 'Umb.EntryPoint.Templating',
		type: 'entrypoint',
		loader: () => import('./index'),
	},
];
