import UmbSortChildrenOfEntityAction from './sort-children-of.action.js';
import { UMB_ENTITY_ACTION_DEFAULT_KIND_MANIFEST } from '@umbraco-cms/backoffice/entity-action';
import type { UmbExtensionManifestKind } from '@umbraco-cms/backoffice/extension-registry';
import { UMB_ACTION_GROUP_STRUCTURE } from '@umbraco-cms/backoffice/action';

export const UMB_ENTITY_ACTION_SORT_CHILDREN_OF_KIND_MANIFEST: UmbExtensionManifestKind = {
	type: 'kind',
	alias: 'Umb.Kind.EntityAction.SortChildrenOf',
	matchKind: 'sortChildrenOf',
	matchType: 'entityAction',
	manifest: {
		...UMB_ENTITY_ACTION_DEFAULT_KIND_MANIFEST.manifest,
		type: 'entityAction',
		kind: 'sortChildrenOf',
		group: UMB_ACTION_GROUP_STRUCTURE,
		api: UmbSortChildrenOfEntityAction,
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
