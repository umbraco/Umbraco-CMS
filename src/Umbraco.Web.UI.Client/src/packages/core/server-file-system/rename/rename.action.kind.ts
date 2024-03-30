import { UMB_ENTITY_ACTION_DEFAULT_KIND_MANIFEST } from '../../entity-action/default/default.action.kind.js';
import type { UmbBackofficeManifestKind } from '@umbraco-cms/backoffice/extension-registry';

export const manifest: UmbBackofficeManifestKind = {
	type: 'kind',
	alias: 'Umb.Kind.EntityAction.Rename',
	matchKind: 'rename',
	matchType: 'entityAction',
	manifest: {
		...UMB_ENTITY_ACTION_DEFAULT_KIND_MANIFEST.manifest,
		type: 'entityAction',
		kind: 'rename',
		api: () => import('./rename.action.js'),
		weight: 200,
		forEntityTypes: [],
		meta: {
			icon: 'icon-edit',
			label: 'Rename...',
			renameRepositoryAlias: '',
			itemRepositoryAlias: '',
		},
	},
};
