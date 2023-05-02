export const name = 'Umbraco.Core';
export const version = '0.0.1';
export const extensions = [
	{
		name: 'Core Entry Point',
		alias: 'Umb.EntryPoint.Core',
		type: 'entryPoint',
		loader: () => import('./index'),
	},
];
