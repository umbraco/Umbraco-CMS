export const name = 'Umbraco.Core.MediaManagement';
export const version = '0.0.1';
export const extensions = [
	{
		name: 'Media Management Entry Point',
		alias: 'Umb.EntryPoint.MediaManagement',
		type: 'entrypoint',
		loader: () => import('./index'),
	},
];
