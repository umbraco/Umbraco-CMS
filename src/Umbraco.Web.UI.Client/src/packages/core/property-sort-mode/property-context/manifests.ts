import type { UmbExtensionManifestKind } from '@umbraco-cms/backoffice/extension-registry';

export const manifests: Array<UmbExtensionManifest | UmbExtensionManifestKind> = [
	{
		type: 'kind',
		alias: 'Umb.Kind.PropertyContext.SortMode',
		matchKind: 'sortMode',
		matchType: 'propertyContext',
		manifest: {
			type: 'propertyContext',
			kind: 'sortMode',
			api: () => import('./property-sort-mode.context.js'),
			weight: 1300,
		},
	},
];
