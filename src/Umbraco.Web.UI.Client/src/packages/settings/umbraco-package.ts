export const name = 'Umbraco.Core.Settings';
export const extensions = [
	{
		name: 'Settings Entry Point',
		alias: 'Umb.EntryPoint.Settings',
		type: 'entryPoint',
		loader: () => import('./index.js'),
	},
];
