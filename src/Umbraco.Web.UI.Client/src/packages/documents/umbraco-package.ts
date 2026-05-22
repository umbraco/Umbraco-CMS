export const name = 'Umbraco.Core.DocumentManagement';
export const extensions = [
	{
		name: 'Document Management Bundle',
		alias: 'Umb.Bundle.DocumentManagement',
		type: 'bundle',
		js: () => import('./manifests.js'),
	},
];
