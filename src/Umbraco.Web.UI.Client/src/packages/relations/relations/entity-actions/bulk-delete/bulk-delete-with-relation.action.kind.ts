import { UMB_ENTITY_BULK_ACTION_DELETE_KIND_MANIFEST } from '@umbraco-cms/backoffice/entity-bulk-action';
import type { UmbExtensionManifestKind } from '@umbraco-cms/backoffice/extension-registry';

export const manifest: UmbExtensionManifestKind = {
	type: 'kind',
	alias: 'Umb.Kind.EntityBulkAction.DeleteWithRelation',
	matchKind: 'deleteWithRelation',
	matchType: 'entityBulkAction',
	manifest: {
		...UMB_ENTITY_BULK_ACTION_DELETE_KIND_MANIFEST.manifest,
		type: 'entityBulkAction',
		kind: 'deleteWithRelation',
		api: () => import('./bulk-delete-with-relation.action.js'),
	},
};
