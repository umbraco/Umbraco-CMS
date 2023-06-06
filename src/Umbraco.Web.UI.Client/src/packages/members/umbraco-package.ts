export const name = 'Umbraco.Core.MemberManagement';
export const extensions = [
	{
		name: 'Member Management Entry Point',
		alias: 'Umb.EntryPoint.MemberManagement',
		type: 'entryPoint',
		loader: () => import('./package-entry-point.js'),
	},
];
