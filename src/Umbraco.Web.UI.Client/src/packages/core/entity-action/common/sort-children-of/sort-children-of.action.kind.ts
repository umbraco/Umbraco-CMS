import { UMB_ENTITY_ACTION_DEFAULT_KIND_MANIFEST } from '../../default/default.action.kind.js';
import type { UmbBackofficeManifestKind } from '@umbraco-cms/backoffice/extension-registry';

export const manifest: UmbBackofficeManifestKind = {
	type: 'kind',
	alias: 'Umb.Kind.EntityAction.Sort',
	matchKind: 'sort',
	matchType: 'entityAction',
	manifest: {
		...UMB_ENTITY_ACTION_DEFAULT_KIND_MANIFEST.manifest,
		type: 'entityAction',
		kind: 'sort',
		api: () => import('./sort-children-of.action.js'),
		weight: 100,
		forEntityTypes: [],
		meta: {
			icon: 'icon-height',
			label: 'Sort...',
			itemRepositoryAlias: '',
			sortRepositoryAlias: '',
		},
	},
};
