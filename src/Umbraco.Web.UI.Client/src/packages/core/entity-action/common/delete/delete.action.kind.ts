import { UMB_ENTITY_ACTION_DEFAULT_KIND_MANIFEST } from '../../default/default.action.kind.js';
import UmbDeleteEntityAction from './delete.action.js';
import type { UmbExtensionManifestKind } from '@umbraco-cms/backoffice/extension-registry';
import { UMB_ACTION_GROUP_DELETE } from '@umbraco-cms/backoffice/action';

export const UMB_ENTITY_ACTION_DELETE_KIND_MANIFEST: UmbExtensionManifestKind = {
	type: 'kind',
	alias: 'Umb.Kind.EntityAction.Delete',
	matchKind: 'delete',
	matchType: 'entityAction',
	manifest: {
		...UMB_ENTITY_ACTION_DEFAULT_KIND_MANIFEST.manifest,
		type: 'entityAction',
		kind: 'delete',
		group: UMB_ACTION_GROUP_DELETE,
		api: UmbDeleteEntityAction,
		weight: 1100,
		forEntityTypes: [],
		meta: {
			icon: 'icon-trash',
			label: '#actions_delete',
			additionalOptions: true,
			itemRepositoryAlias: '',
			detailRepositoryAlias: '',
		},
	},
};

export const manifest = UMB_ENTITY_ACTION_DELETE_KIND_MANIFEST;
