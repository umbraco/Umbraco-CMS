import { UMB_ENTITY_ACTION_DEFAULT_KIND_MANIFEST } from '../../default/default.action.kind.js';
import type { UmbBackofficeManifestKind } from '@umbraco-cms/backoffice/extension-registry';

export const manifest: UmbBackofficeManifestKind = {
	type: 'kind',
	alias: 'Umb.Kind.EntityAction.Move',
	matchKind: 'move',
	matchType: 'entityAction',
	manifest: {
		...UMB_ENTITY_ACTION_DEFAULT_KIND_MANIFEST.manifest,
		type: 'entityAction',
		kind: 'move',
		api: () => import('./move.action.js'),
		weight: 700,
		forEntityTypes: [],
		meta: {
			icon: 'icon-enter',
			label: 'Move to (TBD)...',
			itemRepositoryAlias: '',
			moveRepositoryAlias: '',
			pickerModal: '',
		},
	},
};
