import { UMB_ENTITY_ACTION_DEFAULT_KIND_MANIFEST } from '@umbraco-cms/backoffice/entity-action';
import type { UmbExtensionManifestKind } from '@umbraco-cms/backoffice/extension-registry';
import { UMB_ACTION_GROUP_PUBLISHING } from '@umbraco-cms/backoffice/action';

export const UMB_ENTITY_ACTION_CONTENT_ROLLBACK_KIND_MANIFEST: UmbExtensionManifestKind = {
	type: 'kind',
	alias: 'Umb.Kind.EntityAction.ContentRollback',
	matchKind: 'contentRollback',
	matchType: 'entityAction',
	manifest: {
		...UMB_ENTITY_ACTION_DEFAULT_KIND_MANIFEST.manifest,
		type: 'entityAction',
		kind: 'contentRollback',
		group: UMB_ACTION_GROUP_PUBLISHING,
		api: () => import('./content-rollback.action.js'),
		weight: 450,
		forEntityTypes: [],
		meta: {
			icon: 'icon-history',
			label: '#actions_rollback',
			additionalOptions: true,
			rollbackRepositoryAlias: '',
			detailRepositoryAlias: '',
		},
	},
};

export const manifest = UMB_ENTITY_ACTION_CONTENT_ROLLBACK_KIND_MANIFEST;
