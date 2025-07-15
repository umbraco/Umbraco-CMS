export const name = 'Umbraco.Core.PackageManagement';
export const extensions = [
	{
		name: 'Package Management Bundle',
		alias: 'Umb.Bundle.PackageManagement',
		type: 'bundle',
		js: () => import('./manifests.js'),
	},
];
