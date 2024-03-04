import { UmbDuplicateEntityAction } from './duplicate.action.js';
import type { UmbBackofficeManifestKind } from '@umbraco-cms/backoffice/extension-registry';

export const manifest: UmbBackofficeManifestKind = {
	type: 'kind',
	alias: 'Umb.Kind.EntityAction.Duplicate',
	matchKind: 'duplicate',
	matchType: 'entityAction',
	manifest: {
		type: 'entityAction',
		kind: 'duplicate',
		api: UmbDuplicateEntityAction,
		weight: 600,
		forEntityTypes: [],
		meta: {
			icon: 'icon-documents',
			label: 'Duplicate to...',
			itemRepositoryAlias: '',
			duplicateRepositoryAlias: '',
			pickerModalAlias: '',
		},
	},
};
