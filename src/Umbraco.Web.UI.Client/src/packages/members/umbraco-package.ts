export const name = 'Umbraco.Core.MemberManagement';
export const extensions = [
	{
		name: 'Member Management Bundle',
		alias: 'Umb.Bundle.MemberManagement',
		type: 'bundle',
		loader: () => import('./manifests.js'),
	},
];
