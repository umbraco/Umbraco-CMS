import type { ManifestGlobalContext } from '@umbraco-cms/backoffice/extension-registry';

export const manifests: Array<ManifestGlobalContext> = [
	{
		type: 'globalContext',
		alias: 'Umb.GlobalContext.DocumentConfiguration',
		name: 'Document Configuration Context',
		api: () => import('./document-configuration.context.js'),
	},
];
