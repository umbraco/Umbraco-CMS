export const name = 'Umbraco.Core.DocumentManagement';
export const version = '0.0.1';
export const extensions = [
	{
		name: 'Document Management Entry Point',
		alias: 'Umb.EntryPoint.DocumentManagement',
		type: 'entryPoint',
		loader: () => import('./index'),
	},
];
