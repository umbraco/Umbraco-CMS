export const name = 'Umbraco.Core.Webhooks';
export const extensions = [
	{
		name: 'Webhooks Bundle',
		alias: 'Umb.Bundle.Webhooks',
		type: 'bundle',
		js: () => import('./manifests.js'),
	},
];
