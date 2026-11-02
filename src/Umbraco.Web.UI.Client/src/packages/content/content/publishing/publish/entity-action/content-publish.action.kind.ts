import { UMB_ENTITY_ACTION_DEFAULT_KIND_MANIFEST } from '@umbraco-cms/backoffice/entity-action';
import type { UmbExtensionManifestKind } from '@umbraco-cms/backoffice/extension-registry';

export const UMB_ENTITY_ACTION_CONTENT_PUBLISH_KIND_MANIFEST: UmbExtensionManifestKind = {
	type: 'kind',
	alias: 'Umb.Kind.EntityAction.ContentPublish',
	matchKind: 'contentPublish',
	matchType: 'entityAction',
	manifest: {
		...UMB_ENTITY_ACTION_DEFAULT_KIND_MANIFEST.manifest,
		type: 'entityAction',
		kind: 'contentPublish',
		api: () => import('./content-publish.action.js'),
		weight: 600,
		forEntityTypes: [],
		meta: {
			icon: 'icon-globe',
			label: '#actions_publish',
			additionalOptions: true,
			detailRepositoryAlias: '',
			publishingRepositoryAlias: '',
		},
	},
};

export const manifest = UMB_ENTITY_ACTION_CONTENT_PUBLISH_KIND_MANIFEST;
