export const name = 'Umbraco.Core.DocumentManagement';
export const extensions = [
	{
		name: 'Document Management Entry Point',
		alias: 'Umb.EntryPoint.DocumentManagement',
		type: 'entryPoint',
		loader: () => import('./index'),
	},
];
