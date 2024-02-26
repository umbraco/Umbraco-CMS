import type { ManifestGlobalContext } from '@umbraco-cms/backoffice/extension-registry';

export const manifests: Array<ManifestGlobalContext> = [
	{
		type: 'globalContext',
		alias: 'Umb.GlobalContext.AppWebhook',
		name: 'App Webhook Context',
		js: () => import('./app-webhook.context.js'),
	},
];
