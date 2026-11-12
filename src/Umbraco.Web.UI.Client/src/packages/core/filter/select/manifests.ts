import type { UmbExtensionManifestKind } from '@umbraco-cms/backoffice/extension-registry';

export const manifests: Array<UmbExtensionManifest | UmbExtensionManifestKind> = [
	{
		type: 'kind',
		alias: 'Umb.Kind.Filter.Select',
		matchKind: 'select',
		matchType: 'filter',
		manifest: {
			type: 'filter',
			kind: 'select',
			element: () => import('./select-filter.element.js'),
			api: () => import('./select-filter.api.js'),
		},
	},
];
