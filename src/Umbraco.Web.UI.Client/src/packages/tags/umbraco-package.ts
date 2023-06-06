export const name = 'Umbraco.Core.UserManagement';
export const version = '0.0.1';
export const extensions = [
	{
		name: 'Tags Management Entry Point',
		alias: 'Umb.EntryPoint.TagsManagement',
		type: 'entryPoint',
		loader: () => import('./package-entry-point.js'),
	},
];
