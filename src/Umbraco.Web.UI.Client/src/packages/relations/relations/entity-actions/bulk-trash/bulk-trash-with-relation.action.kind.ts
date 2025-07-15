import type { UmbExtensionManifestKind } from '@umbraco-cms/backoffice/extension-registry';
import { UMB_ENTITY_BULK_ACTION_TRASH_KIND_MANIFEST } from '@umbraco-cms/backoffice/recycle-bin';

export const UMB_ENTITY_BULK_ACTION_TRASH_WITH_RELATION_KIND = 'trashWithRelation';

export const manifest: UmbExtensionManifestKind = {
	type: 'kind',
	alias: 'Umb.Kind.EntityBulkAction.TrashWithRelation',
	matchKind: 'trashWithRelation',
	matchType: 'entityBulkAction',
	manifest: {
		...UMB_ENTITY_BULK_ACTION_TRASH_KIND_MANIFEST.manifest,
		type: 'entityBulkAction',
		kind: 'trashWithRelation',
		api: () => import('./bulk-trash-with-relation.action.js'),
	},
};
