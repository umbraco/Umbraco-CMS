import type { UmbExtensionManifestKind } from '@umbraco-cms/backoffice/extension-registry';

export const UMB_VALUE_SUMMARY_DEFAULT_KIND_MANIFEST: UmbExtensionManifestKind = {
	type: 'kind',
	alias: 'Umb.Kind.ValueSummary.Default',
	matchKind: 'default',
	matchType: 'valueSummary',
	manifest: {
		type: 'valueSummary',
		kind: 'default',
		element: () => import('./default-value-summary.js'),
		api: () => import('./default-value-summary.js'),
		meta: {},
	},
};

export const manifest = UMB_VALUE_SUMMARY_DEFAULT_KIND_MANIFEST;
