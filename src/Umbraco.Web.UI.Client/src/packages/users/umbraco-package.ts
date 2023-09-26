export const name = 'Umbraco.Core.UserManagement';
export const version = '0.0.1';
export const extensions = [
	{
		name: 'User Management Bundle',
		alias: 'Umb.Bundle.UserManagement',
		type: 'bundle',
		loader: () => import('./manifests.js'),
	},
	{
		name: 'User Management Entry Point',
		alias: 'Umb.EntryPoint.UserManagement',
		type: 'entryPoint',
		loader: () => import('./index.js'),
	},
];
