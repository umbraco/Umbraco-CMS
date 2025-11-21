import type { UmbExtensionManifestKind } from '@umbraco-cms/backoffice/extension-registry';

export const manifests: Array<UmbExtensionManifest | UmbExtensionManifestKind> = [
	{
		type: 'kind',
		alias: 'Umb.Kind.PropertyContext.Sort',
		matchKind: 'sort',
		matchType: 'propertyContext',
		manifest: {
			type: 'propertyContext',
			kind: 'sort',
			api: () => import('./sort.property-context.js'),
			weight: 1300,
		},
	},
];
