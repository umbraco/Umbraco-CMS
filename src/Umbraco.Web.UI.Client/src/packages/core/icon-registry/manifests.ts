import type { ManifestTypes } from '@umbraco-cms/backoffice/extension-registry';

export const manifests: Array<ManifestTypes> = [
	{
		type: 'icons',
		alias: 'Umb.Icons.Backoffice',
		name: 'Backoffice Icons',
		js: () => import('./icons/icons.js'),
	},
	{
		type: 'globalContext',
		alias: 'Umb.GlobalContext.Icons',
		name: 'Icons Context',
		api: () => import('./icon-registry.context.js'),
	},
];
