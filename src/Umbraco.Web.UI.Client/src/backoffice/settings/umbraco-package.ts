export const name = 'Umbraco.Core.Settings';
export const version = '0.0.1';
export const extensions = [
	{
		name: 'Settings Entry Point',
		alias: 'Umb.EntryPoint.Settings',
		type: 'entrypoint',
		loader: () => import('./index'),
	},
];
