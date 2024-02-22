import type { ManifestGlobalContext } from '@umbraco-cms/backoffice/extension-registry';

export const manifests: Array<ManifestGlobalContext> = [
	{
		type: 'globalContext',
		alias: 'Umb.GlobalContext.DocumentVariantManager',
		name: 'Document Variant Manager Context',
		js: () => import('./document-variant-manager.context.js'),
	},
];
