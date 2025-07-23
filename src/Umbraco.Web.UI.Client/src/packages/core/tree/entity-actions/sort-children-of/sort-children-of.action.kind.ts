import { UMB_ENTITY_ACTION_DEFAULT_KIND_MANIFEST } from '@umbraco-cms/backoffice/entity-action';
import type { UmbExtensionManifestKind } from '@umbraco-cms/backoffice/extension-registry';

export const UMB_ENTITY_ACTION_SORT_CHILDREN_OF_KIND_MANIFEST: UmbExtensionManifestKind = {
	type: 'kind',
	alias: 'Umb.Kind.EntityAction.SortChildrenOf',
	matchKind: 'sortChildrenOf',
	matchType: 'entityAction',
	manifest: {
		...UMB_ENTITY_ACTION_DEFAULT_KIND_MANIFEST.manifest,
		type: 'entityAction',
		kind: 'sortChildrenOf',
		api: () => import('./sort-children-of.action.js'),
		weight: 100,
		forEntityTypes: [],
		meta: {
			icon: 'icon-height',
			label: '#actions_sort',
			additionalOptions: true,
			itemRepositoryAlias: '',
			sortRepositoryAlias: '',
		},
	},
};

export const manifest = UMB_ENTITY_ACTION_SORT_CHILDREN_OF_KIND_MANIFEST;
