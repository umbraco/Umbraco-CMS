export const name = 'Umbraco.Core.Webhook';
export const extensions = [
	{
		name: 'Webhook Bundle',
		alias: 'Umb.Bundle.Webhook',
		type: 'bundle',
		js: () => import('./manifests.js'),
	},
];
