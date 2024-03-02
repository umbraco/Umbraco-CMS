import type { ManifestGlobalContext } from '@umbraco-cms/backoffice/extension-registry';

export const manifests: Array<ManifestGlobalContext> = [
	{
		type: 'globalContext',
		alias: 'Umb.GlobalContext.AppLanguage',
		name: 'App Language Context',
		js: () => import('./app-language.context.js'),
	},
];
