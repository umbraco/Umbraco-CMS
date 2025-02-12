import type { UmbExtensionManifestKind } from '@umbraco-cms/backoffice/extension-registry';

export const UMB_ENTITY_ACTION_DEFAULT_KIND_MANIFEST: UmbExtensionManifestKind = {
	type: 'kind',
	alias: 'Umb.Kind.EntityAction.Default',
	matchKind: 'default',
	matchType: 'entityAction',
	manifest: {
		type: 'entityAction',
		kind: 'default',
		weight: 1000,
		element: () => import('./entity-action.element.js'),
		meta: {
			icon: '',
			label: 'Default Entity Action',
		},
	},
};

export const manifest = UMB_ENTITY_ACTION_DEFAULT_KIND_MANIFEST;
