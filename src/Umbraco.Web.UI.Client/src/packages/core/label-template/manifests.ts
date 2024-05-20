import type { ManifestGlobalContext } from '@umbraco-cms/backoffice/extension-registry';

export const manifests: Array<ManifestGlobalContext> = [
	{
		type: 'globalContext',
		alias: 'Umb.GlobalContext.LabelTemplate',
		name: 'Label Template Global Context',
		api: () => import('./label-template.context.js'),
	},
];
