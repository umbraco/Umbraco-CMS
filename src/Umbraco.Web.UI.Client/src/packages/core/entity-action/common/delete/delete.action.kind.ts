import { UMB_ENTITY_ACTION_DEFAULT_KIND_MANIFEST } from '../../default/default.action.kind.js';
import { UMB_ENTITY_ACTION_GROUP_STRUCTURE } from '../../action-groups/constants.js';
import UmbDeleteEntityAction from './delete.action.js';
import type { UmbExtensionManifestKind } from '@umbraco-cms/backoffice/extension-registry';

export const UMB_ENTITY_ACTION_DELETE_KIND_MANIFEST: UmbExtensionManifestKind = {
	type: 'kind',
	alias: 'Umb.Kind.EntityAction.Delete',
	matchKind: 'delete',
	matchType: 'entityAction',
	manifest: {
		...UMB_ENTITY_ACTION_DEFAULT_KIND_MANIFEST.manifest,
		type: 'entityAction',
		group: UMB_ENTITY_ACTION_GROUP_STRUCTURE,
		kind: 'delete',
		api: UmbDeleteEntityAction,
		weight: 799,
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
