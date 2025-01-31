export const name = 'Umbraco.Core.TagManagement';
export const version = '0.0.1';
export const extensions = [
	{
		name: 'Tags Management Bundle',
		alias: 'Umb.Bundle.TagsManagement',
		type: 'bundle',
		js: () => import('./manifests.js'),
	},
];
