export const name = 'Umbraco.Core.MediaManagement';
export const extensions = [
	{
		name: 'Media Management Entry Point',
		alias: 'Umb.EntryPoint.MediaManagement',
		type: 'entryPoint',
		loader: () => import('./index'),
	},
];
