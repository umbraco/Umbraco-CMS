export const name = 'Umbraco.Core.DocumentManagement';
export const extensions = [
	{
		name: 'Block Management Bundle',
		alias: 'Umb.Bundle.BlockManagement',
		type: 'bundle',
		js: () => import('./manifests.js'),
	},
];
