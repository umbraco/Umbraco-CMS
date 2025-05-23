import { UMB_ENTITY_BULK_ACTION_DEFAULT_KIND_MANIFEST } from '../../default/default.action.kind.js';
import type { UmbExtensionManifestKind } from '@umbraco-cms/backoffice/extension-registry';

export const manifest: UmbExtensionManifestKind = {
	type: 'kind',
	alias: 'Umb.Kind.EntityBulkAction.DuplicateTo',
	matchKind: 'duplicateTo',
	matchType: 'entityBulkAction',
	manifest: {
		...UMB_ENTITY_BULK_ACTION_DEFAULT_KIND_MANIFEST.manifest,
		type: 'entityBulkAction',
		kind: 'duplicateTo',
		api: () => import('./duplicate-to.action.js'),
		weight: 700,
		forEntityTypes: [],
		meta: {
			icon: 'icon-enter',
			label: '#actions_copyTo',
			bulkDuplicateRepositoryAlias: '',
			treeAlias: '',
		},
	},
};
