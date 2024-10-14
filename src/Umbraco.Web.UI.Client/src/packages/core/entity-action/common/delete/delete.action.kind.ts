import { UMB_ENTITY_ACTION_DEFAULT_KIND_MANIFEST } from '../../default/default.action.kind.js';
import type { UmbExtensionManifestKind } from '@umbraco-cms/backoffice/extension-registry';

export const manifest: UmbExtensionManifestKind = {
	type: 'kind',
	alias: 'Umb.Kind.EntityAction.Delete',
	matchKind: 'delete',
	matchType: 'entityAction',
	manifest: {
		...UMB_ENTITY_ACTION_DEFAULT_KIND_MANIFEST.manifest,
		type: 'entityAction',
		kind: 'delete',
		api: () => import('./delete.action.js'),
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
