export const name = 'Umbraco.Core.UserManagement';
export const version = '0.0.1';
export const extensions = [
	{
		name: 'Tags Management Bundle',
		alias: 'Umb.Bundle.TagsManagement',
		type: 'bundle',
		loader: () => import('./manifests.js'),
	},
];
