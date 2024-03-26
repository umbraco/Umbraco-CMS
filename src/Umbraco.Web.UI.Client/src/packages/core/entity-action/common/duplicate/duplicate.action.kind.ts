import { UMB_ENTITY_ACTION_DEFAULT_KIND_MANIFEST } from '../../default/default.action.kind.js';
import type { UmbBackofficeManifestKind } from '@umbraco-cms/backoffice/extension-registry';

export const manifest: UmbBackofficeManifestKind = {
	type: 'kind',
	alias: 'Umb.Kind.EntityAction.Duplicate',
	matchKind: 'duplicate',
	matchType: 'entityAction',
	manifest: {
		...UMB_ENTITY_ACTION_DEFAULT_KIND_MANIFEST.manifest,
		type: 'entityAction',
		kind: 'duplicate',
		api: () => import('./duplicate.action.js'),
		weight: 600,
		forEntityTypes: [],
		meta: {
			icon: 'icon-documents',
			label: 'Duplicate to...',
			itemRepositoryAlias: '',
			duplicateRepositoryAlias: '',
			pickerModal: '',
		},
	},
};
