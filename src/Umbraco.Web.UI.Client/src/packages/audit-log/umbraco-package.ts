export const name = 'Umbraco.Core.AuditLog';
export const extensions = [
	{
		name: 'Audit Log Bundle',
		alias: 'Umb.Bundle.AuditLog',
		type: 'bundle',
		js: () => import('./manifests.js'),
	},
];
