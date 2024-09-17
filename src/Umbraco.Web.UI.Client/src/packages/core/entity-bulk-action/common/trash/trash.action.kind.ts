import { UMB_ENTITY_BULK_ACTION_DEFAULT_KIND_MANIFEST } from '../../default/default.action.kind.js';
import type { UmbExtensionManifestKind } from '@umbraco-cms/backoffice/extension-registry';

export const manifest: UmbExtensionManifestKind = {
	type: 'kind',
	alias: 'Umb.Kind.EntityBulkAction.Trash',
	matchKind: 'trash',
	matchType: 'entityBulkAction',
	manifest: {
		...UMB_ENTITY_BULK_ACTION_DEFAULT_KIND_MANIFEST.manifest,
		type: 'entityBulkAction',
		kind: 'trash',
		api: () => import('./trash.action.js'),
		weight: 700,
		forEntityTypes: [],
		meta: {
			icon: 'icon-trash',
			label: '#actions_trash',
			bulkTrashRepositoryAlias: '',
		},
	},
};
