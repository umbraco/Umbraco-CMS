import { UMB_ENTITY_ACTION_DEFAULT_KIND_MANIFEST } from '@umbraco-cms/backoffice/entity-action';
import type { UmbExtensionManifestKind } from '@umbraco-cms/backoffice/extension-registry';
import { UmbTrashEntityAction } from './trash.action.js';

export const UMB_ENTITY_ACTION_TRASH_KIND_MANIFEST: UmbExtensionManifestKind = {
	type: 'kind',
	alias: 'Umb.Kind.EntityAction.Trash',
	matchKind: 'trash',
	matchType: 'entityAction',
	manifest: {
		...UMB_ENTITY_ACTION_DEFAULT_KIND_MANIFEST.manifest,
		type: 'entityAction',
		kind: 'trash',
		api: UmbTrashEntityAction,
		weight: 1150,
		meta: {
			icon: 'icon-trash',
			label: '#actions_trash',
			additionalOptions: true,
		},
	},
};

export const manifest = UMB_ENTITY_ACTION_TRASH_KIND_MANIFEST;
