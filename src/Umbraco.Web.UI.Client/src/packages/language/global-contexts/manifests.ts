import type { ManifestGlobalContext } from '@umbraco-cms/backoffice/extension-registry';

export const manifests: Array<ManifestGlobalContext> = [
	{
		type: 'globalContext',
		alias: 'Umb.GlobalContext.AppLanguage',
		name: 'App Language Context',
		api: () => import('./app-language.context.js'),
	},
];
