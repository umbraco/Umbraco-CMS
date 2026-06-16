import { UMB_ENTITY_BULK_ACTION_DEFAULT_KIND_MANIFEST } from '../../default/default.action.kind.js';
import type { UmbExtensionManifestKind } from '@umbraco-cms/backoffice/extension-registry';
import { UmbDeleteEntityBulkAction } from './bulk-delete.action.js';

export const UMB_ENTITY_BULK_ACTION_DELETE_KIND = 'delete';

export const UMB_ENTITY_BULK_ACTION_DELETE_KIND_MANIFEST: UmbExtensionManifestKind = {
	type: 'kind',
	alias: 'Umb.Kind.EntityBulkAction.Delete',
	matchKind: UMB_ENTITY_BULK_ACTION_DELETE_KIND,
	matchType: 'entityBulkAction',
	manifest: {
		...UMB_ENTITY_BULK_ACTION_DEFAULT_KIND_MANIFEST.manifest,
		type: 'entityBulkAction',
		kind: UMB_ENTITY_BULK_ACTION_DELETE_KIND,
		api: UmbDeleteEntityBulkAction,
		weight: 1100,
		meta: {
			icon: 'icon-trash',
			label: '#actions_delete',
		},
	},
};

export const manifest = UMB_ENTITY_BULK_ACTION_DELETE_KIND_MANIFEST;
