import {
	UMB_ENTITY_ACTION_DEFAULT_KIND_MANIFEST,
	UMB_ENTITY_ACTION_GROUP_PUBLISHING,
} from '@umbraco-cms/backoffice/entity-action';
import type { UmbExtensionManifestKind } from '@umbraco-cms/backoffice/extension-registry';

export const UMB_ENTITY_ACTION_CONTENT_UNPUBLISH_KIND_MANIFEST: UmbExtensionManifestKind = {
	type: 'kind',
	alias: 'Umb.Kind.EntityAction.ContentUnpublish',
	matchKind: 'contentUnpublish',
	matchType: 'entityAction',
	manifest: {
		...UMB_ENTITY_ACTION_DEFAULT_KIND_MANIFEST.manifest,
		type: 'entityAction',
		group: UMB_ENTITY_ACTION_GROUP_PUBLISHING,
		kind: 'contentUnpublish',
		api: () => import('./content-unpublish.action.js'),
		weight: 500,
		forEntityTypes: [],
		meta: {
			icon: 'icon-globe',
			label: '#actions_unpublish',
			additionalOptions: true,
			detailRepositoryAlias: '',
			publishingRepositoryAlias: '',
			itemRepositoryAlias: '',
			referenceRepositoryAlias: '',
			configurationRepositoryAlias: '',
		},
	},
};

export const manifest = UMB_ENTITY_ACTION_CONTENT_UNPUBLISH_KIND_MANIFEST;
