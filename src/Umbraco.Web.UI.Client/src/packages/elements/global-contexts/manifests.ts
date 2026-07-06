import type { ManifestGlobalContext } from '@umbraco-cms/backoffice/extension-registry';

export const manifests: Array<ManifestGlobalContext> = [
	{
		type: 'globalContext',
		alias: 'Umb.GlobalContext.ElementConfiguration',
		name: 'Element Configuration Context',
		api: () => import('./element-configuration.context.js'),
	},
];
