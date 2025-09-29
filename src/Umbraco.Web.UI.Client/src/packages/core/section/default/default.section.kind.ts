import type { UmbExtensionManifestKind } from '@umbraco-cms/backoffice/extension-registry';

export const manifest: UmbExtensionManifestKind = {
	type: 'kind',
	alias: 'Umb.Kind.Section.Default',
	matchKind: 'default',
	matchType: 'section',
	manifest: {
		type: 'section',
		kind: 'default',
		weight: 1000,
		api: () => import('./default-section.context.js'),
		element: () => import('./default-section.element.js'),
		meta: {
			label: '',
			pathname: '',
		},
	},
};
