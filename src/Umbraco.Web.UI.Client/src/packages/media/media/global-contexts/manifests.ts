import type { ManifestGlobalContext } from '@umbraco-cms/backoffice/extension-registry';

export const manifests: Array<ManifestGlobalContext> = [
	{
		type: 'globalContext',
		alias: 'Umb.GlobalContext.MediaConfiguration',
		name: 'Media Configuration Context',
		api: () => import('./media-configuration.context.js'),
	},
];
