export const name = 'Umbraco.Core.MemberManagement';
export const extensions = [
	{
		name: 'Member Management Bundle',
		alias: 'Umb.Bundle.MemberManagement',
		type: 'bundle',
		js: () => import('./manifests.js'),
	},
];
