export const name = 'Umbraco.Core.UserManagement';
export const version = '0.0.1';
export const extensions = [
	{
		name: 'User Management Bundle',
		alias: 'Umb.Bundle.UserManagement',
		type: 'bundle',
		js: () => import('./manifests.js'),
	},
];
