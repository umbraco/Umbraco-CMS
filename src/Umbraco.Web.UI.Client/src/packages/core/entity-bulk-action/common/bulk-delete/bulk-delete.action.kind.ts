import { UMB_ENTITY_BULK_ACTION_DEFAULT_KIND_MANIFEST } from '@umbraco-cms/backoffice/entity-bulk-action';
import type { UmbExtensionManifestKind } from '@umbraco-cms/backoffice/extension-registry';

export const UMB_ENTITY_BULK_ACTION_DELETE_KIND_MANIFEST: UmbExtensionManifestKind = {
	type: 'kind',
	alias: 'Umb.Kind.EntityBulkAction.Delete',
	matchKind: 'delete',
	matchType: 'entityBulkAction',
	manifest: {
		...UMB_ENTITY_BULK_ACTION_DEFAULT_KIND_MANIFEST.manifest,
		type: 'entityBulkAction',
		kind: 'delete',
		api: () => import('./bulk-delete.action.js'),
		weight: 1100,
		meta: {
			icon: 'icon-trash',
			label: '#actions_delete',
		},
	},
};

export const manifest = UMB_ENTITY_BULK_ACTION_DELETE_KIND_MANIFEST;
