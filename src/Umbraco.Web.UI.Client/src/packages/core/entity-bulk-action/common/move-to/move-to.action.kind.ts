import { UMB_ENTITY_BULK_ACTION_DEFAULT_KIND_MANIFEST } from '../../default/default.action.kind.js';
import type { UmbExtensionManifestKind } from '@umbraco-cms/backoffice/extension-registry';
import { UmbMoveToEntityBulkAction } from './move-to.action.js';

export const manifest: UmbExtensionManifestKind = {
	type: 'kind',
	alias: 'Umb.Kind.EntityBulkAction.MoveTo',
	matchKind: 'moveTo',
	matchType: 'entityBulkAction',
	manifest: {
		...UMB_ENTITY_BULK_ACTION_DEFAULT_KIND_MANIFEST.manifest,
		type: 'entityBulkAction',
		kind: 'moveTo',
		api: UmbMoveToEntityBulkAction,
		weight: 700,
		forEntityTypes: [],
		meta: {
			icon: 'icon-enter',
			label: '#actions_move',
			bulkMoveRepositoryAlias: '',
			treeAlias: '',
		},
	},
};
