import type { UmbExtensionManifestKind } from '@umbraco-cms/backoffice/extension-registry';

export const manifests: Array<UmbExtensionManifest | UmbExtensionManifestKind> = [
	{
		type: 'kind',
		alias: 'Example.Kind.Filter.Range',
		matchKind: 'range',
		matchType: 'filter',
		manifest: {
			type: 'filter',
			kind: 'range',
			element: () => import('./range-filter.element.js'),
			api: () => import('./range-filter.api.js'),
		},
	},
];
