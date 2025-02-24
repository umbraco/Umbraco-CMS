import { UMB_ENTITY_BULK_ACTION_DEFAULT_KIND_MANIFEST } from '@umbraco-cms/backoffice/entity-bulk-action';
import type { UmbExtensionManifestKind } from '@umbraco-cms/backoffice/extension-registry';

/* TODO: v17: rename kind to trash
 this is named v2 to avoid a name clash. The original trash kind is deprecated.
We have added a constant to try and prevent too big a breaking change when renaming. */
export const UMB_ENTITY_BULK_ACTION_TRASH_KIND = 'trashV2';

export const UMB_ENTITY_BULK_ACTION_TRASH_KIND_MANIFEST: UmbExtensionManifestKind = {
	type: 'kind',
	alias: 'Umb.Kind.EntityBulkAction.Trash',
	matchKind: UMB_ENTITY_BULK_ACTION_TRASH_KIND,
	matchType: 'entityBulkAction',
	manifest: {
		...UMB_ENTITY_BULK_ACTION_DEFAULT_KIND_MANIFEST.manifest,
		type: 'entityBulkAction',
		kind: UMB_ENTITY_BULK_ACTION_TRASH_KIND,
		api: () => import('./bulk-trash.action.js'),
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
