export const name = 'Umbraco.Core.Templating';
export const version = '0.0.1';
export const extensions = [
	{
		type: 'entryPoint',
		loader: () => import('./index'),
	},
];
