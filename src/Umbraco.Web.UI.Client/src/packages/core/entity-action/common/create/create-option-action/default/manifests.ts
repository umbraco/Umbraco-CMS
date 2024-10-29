import type { UmbExtensionManifestKind } from '@umbraco-cms/backoffice/extension-registry';

export const manifests: Array<UmbExtensionManifest | UmbExtensionManifestKind> = [
	{
		type: 'kind',
		alias: 'Umb.Kind.EntityCreateOptionAction.Default',
		matchKind: 'default',
		matchType: 'entityCreateOptionAction',
		manifest: {
			type: 'entityCreateOptionAction',
			kind: 'default',
			weight: 1000,
			element: () => import('./entity-create-option-action.element.js'),
			meta: {
				icon: '',
				label: 'Default Entity Create Option Action',
			},
		},
	},
];
