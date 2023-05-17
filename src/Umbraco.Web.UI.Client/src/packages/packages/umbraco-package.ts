export const name = 'Umbraco.Core.PackageManagement';
export const extensions = [
	{
		name: 'Package Management Entry Point',
		alias: 'Umb.EntryPoint.PackageManagement',
		type: 'entryPoint',
		loader: () => import('./index'),
	},
];
