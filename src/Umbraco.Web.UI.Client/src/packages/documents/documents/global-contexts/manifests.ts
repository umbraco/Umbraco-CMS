import type { ManifestGlobalContext } from '@umbraco-cms/backoffice/extension-registry';

export const manifests: Array<ManifestGlobalContext> = [
	{
		type: 'globalContext',
		alias: 'Umb.GlobalContext.DocumentConfiguration',
		name: 'Document Configuration Context',
		js: () => import('./document-configuration.context.js'),
	},
];
