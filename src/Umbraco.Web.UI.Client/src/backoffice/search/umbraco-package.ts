export const name = 'Umbraco.Core.Search';
export const version = '0.0.1';
export const extensions = [
	{
		name: 'Search Entry Point',
		alias: 'Umb.EntryPoint.Search',
		type: 'entryPoint',
		loader: () => import('./index'),
	},
];
