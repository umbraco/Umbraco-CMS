export const name = 'Umbraco.Core.Search';
export const extensions = [
	{
		name: 'Search Entry Point',
		alias: 'Umb.EntryPoint.Search',
		type: 'entryPoint',
		loader: () => import('./package-entry-point.js'),
	},
];
