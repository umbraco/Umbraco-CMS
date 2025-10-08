import type { UmbExtensionManifestKind } from '@umbraco-cms/backoffice/extension-registry';

export const manifests: Array<UmbExtensionManifest | UmbExtensionManifestKind> = [
	{
		type: 'kind',
		alias: 'Umb.Kind.Section.Default',
		matchKind: 'default',
		matchType: 'section',
		manifest: {
			type: 'section',
			kind: 'default',
			weight: 1000,
			element: () => import('./default-section.element.js'),
			meta: {
				label: '',
				pathname: '',
			},
		},
	},
];
