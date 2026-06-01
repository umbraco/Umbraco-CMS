import { UMB_ENTITY_BULK_ACTION_DEFAULT_KIND_MANIFEST } from '@umbraco-cms/backoffice/entity-bulk-action';
import type { UmbExtensionManifestKind } from '@umbraco-cms/backoffice/extension-registry';
import { UmbTrashEntityBulkAction } from './bulk-trash.action.js';

export const UMB_ENTITY_BULK_ACTION_TRASH_KIND = 'trash';

export const UMB_ENTITY_BULK_ACTION_TRASH_KIND_MANIFEST: UmbExtensionManifestKind = {
	type: 'kind',
	alias: 'Umb.Kind.EntityBulkAction.Trash',
	matchKind: UMB_ENTITY_BULK_ACTION_TRASH_KIND,
	matchType: 'entityBulkAction',
	manifest: {
		...UMB_ENTITY_BULK_ACTION_DEFAULT_KIND_MANIFEST.manifest,
		type: 'entityBulkAction',
		kind: UMB_ENTITY_BULK_ACTION_TRASH_KIND,
		api: UmbTrashEntityBulkAction,
		weight: 1150,
		meta: {
			icon: 'icon-trash',
			label: '#actions_trash',
			itemRepositoryAlias: '',
			recycleBinRepositoryAlias: '',
		},
	},
};

export const manifest = UMB_ENTITY_BULK_ACTION_TRASH_KIND_MANIFEST;
