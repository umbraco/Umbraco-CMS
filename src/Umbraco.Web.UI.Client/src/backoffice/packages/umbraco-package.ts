export const name = 'Umbraco.Core.PackageManagement';
export const version = '0.0.1';
export const extensions = [
	{
		name: 'Package Management Entry Point',
		alias: 'Umb.EntryPoint.PackageManagement',
		type: 'entryPoint',
		loader: () => import('./index'),
	},
];
