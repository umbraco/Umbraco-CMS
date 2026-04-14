import type { UmbExtensionManifestKind } from '@umbraco-cms/backoffice/extension-registry';

export const manifests: Array<UmbExtensionManifest | UmbExtensionManifestKind> = [
	{
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
	},
];
