import type { UmbBackofficeManifestKind } from '@umbraco-cms/backoffice/extension-registry';

export const manifest: UmbBackofficeManifestKind = {
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
