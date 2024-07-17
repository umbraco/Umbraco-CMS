import { UMB_ENTITY_BULK_ACTION_DEFAULT_KIND_MANIFEST } from '../../default/default.action.kind.js';
import type { UmbBackofficeManifestKind } from '@umbraco-cms/backoffice/extension-registry';

export const manifest: UmbBackofficeManifestKind = {
	type: 'kind',
	alias: 'Umb.Kind.EntityBulkAction.MoveTo',
	matchKind: 'moveTo',
	matchType: 'entityBulkAction',
	manifest: {
		...UMB_ENTITY_BULK_ACTION_DEFAULT_KIND_MANIFEST.manifest,
		type: 'entityBulkAction',
		kind: 'moveTo',
		api: () => import('./move-to.action.js'),
		weight: 700,
		forEntityTypes: [],
		meta: {
			label: '#actions_move',
			bulkMoveRepositoryAlias: '',
			treeAlias: '',
		},
	},
};
